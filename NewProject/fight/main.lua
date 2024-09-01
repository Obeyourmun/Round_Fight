local Hero = require("LuaXlsx.Hero")
local Skill = require("LuaXlsx.Skill")
local Buff = require("LuaXlsx.Buff")
local skill = require("fight.skill")
local Logger = require("fight.log")
local fight = {}

--常量声明
local begin_round = 1 --初始回合数
local max_round = 8 --最大回合数
local max_skills_num = 3 --最大装备技能数

-- buff列表
local NORMAL_ATTACK_BUFF = 1 -- 普通攻击 
local PHY_HURT_BUFF = 2 -- 兵刃直伤
local MAG_HURT_BUFF = 3 -- 谋略直伤
local FIRE_BUFF = 4 -- 火攻 智力下降20
local FLOOD_BUFF = 5 -- 洪水 统帅下降20
local STORM_BUFF = 6 -- 风暴 速度下降20
local RECOVER_BUFF = 7 -- 回血效果 
local KILL_ACTIVE_BUFF = 8 -- 技穷 主动技不发动
local KILL_GRAIN_BUFF = 9 -- 断粮 回血生效30
local WEAK_BUFF = 10 -- 虚弱 造成伤害生效30
local DISARM_BUFF = 11 -- 缴械 不能普攻

-- 触发位置
local ROUND_BEGIN_TIME = 1 -- 回合开始时
local RUN_BEGIN_TIME = 2 -- 行动前
local ACTIVE_TIME = 3 -- 主动技
local NORMAL_ATTACK_TIME = 4 -- 普攻
local ATTACK_NORMAL_TIME = 5 -- 追击(普攻后)
local ROUND_END_TIME = 6 -- 回合结束时

-- LOG
local LOG_TYPE_4 = 4
local LOG_TYPE_5 = 5
local LOG_TYPE_6 = 6
local LOG_TYPE_7 = 7
local LOG_TYPE_12 = 12
local LOG_TYPE_13 = 13
local LOG_TYPE_14 = 14

-- 技能类型
SKILL_TYPE_COMMAND = 1 -- 指挥
SKILL_TYPE_ACTIVE = 2 -- 主动
SKILL_TYPE_NOR_ATK = 3 -- 普攻
SKILL_TYPE_PURSUIT = 4 -- 追击
SKILL_TYPE_PASSIVE = 5 -- 被动


local NOT_CONTINOUS = 0 -- 不持续的buff 
local CONTINOUS = 1 -- 持续的buff 



-- 战斗模块
function fight.main(player_a, player_b)
    local is_end, tip, tip2, die_num, die_num2 = false, 0, 0, 0, 0

    -- 初始化数据
    local rolea, roleb = init_role(player_a, player_b)

    local role_list, heroa_num, herob_num = prepare_round(rolea, roleb)

    -- 回合循环
    for a = begin_round, max_round, 1 do  --max_round
        -- 计算行动顺序
        local speed_list = calculate_speed(role_list)

        local i = 1
        local warlords_num = #speed_list
        while i <= warlords_num do  -- 每个玩家轮流行动
            -- 回合开始
            round_begin_tri()

            -- 行动前
            run_begin_tri()

            -- 主动技能
            is_end, tip2, die_num2 = active_tri(speed_list[i], speed_list)
            tip, die_num = tip+tip2, die_num+die_num2
            if is_end == true then
                --print("game over") 
                Logger.log(LOG_TYPE_7) 
            return end

            -- 普攻
            is_end, tip2, die_num2 = normal_attack_tri(speed_list[i], speed_list)
            tip, die_num = tip+tip2, die_num+die_num2
            if is_end == true then 
                --print("game over") 
                Logger.log(LOG_TYPE_7)
            return end

            -- 追击
            is_end, tip2, die_num2 = atk_normal_tri(speed_list[i], speed_list)
            tip, die_num = tip+tip2, die_num+die_num2
            if is_end == true then 
                --print("game over") 
                Logger.log(LOG_TYPE_7)
            return end

            -- 回合结束前
            round_end_tri()

            i = i - tip
            warlords_num = warlords_num - die_num
            i = i + 1
        end

        -- buff回收
        i = 1
        while i <= #speed_list do
            is_end, tip2, die_num2 = buff_recycle(speed_list[i], speed_list)
            tip, die_num = tip+tip2, die_num+die_num2
            if is_end == true then 
                -- print("game over") 
                Logger.log(LOG_TYPE_7)
            return end
            i = i + 1
        end

        -- print()
        Logger.log(LOG_TYPE_12)
    end

    Logger.log2()
end

-- 准备回合
function prepare_round(rolea, roleb)
    local role_list = {}
    local hero_num_a = 0
    local hero_num_b = 0

    for a, hero in ipairs(rolea.heros) do
        table.insert(role_list, hero)
        hero_num_a = hero_num_a + 1
    end

    for a, hero in ipairs(roleb.heros) do
        table.insert(role_list, hero)
        hero_num_b = hero_num_b + 1
    end

    return role_list, hero_num_a, hero_num_b
end

-- 计算行动顺序
function calculate_speed(speed_list)

    table.sort(speed_list, function (a, b)
        return a.speed > b.speed
    end)

    for a, hero in ipairs(speed_list) do
        hero.run_rank = a
        --print(a, hero.heroid)
    end

    return speed_list
end

function round_begin_tri()
    
end

function run_begin_tri()
    
end

-- 主动技能触发
function active_tri(hero, hero_list)
    local is_end, tip, die_num = false, 0, 0

    for a=1, max_skills_num, 1 do

        local result = is_tri_time(hero, ACTIVE_TIME, a)
        if result == true then
            local skill_id = hero.skills[a]
            local buff_list = Skill[skill_id].attribute
            local tip2, die_num2 = 0, 0

            -- 循环处理每个技能触发buff
            for i, buff in pairs(buff_list) do
                local result2 = is_can_tri(buff)
                if result2 == true then
                    -- buff埋点
                    -- 兵刃直伤buff
                    if buff[1] == PHY_HURT_BUFF then 
                        is_end, tip2, die_num2 = skill.phy_hurt(hero, hero_list, buff) 
                    end
                    -- 谋略直伤buff
                    if buff[1] == MAG_HURT_BUFF then 
                        is_end, tip2, die_num2 = skill.mag_hurt(hero, hero_list, buff) 
                    end
                    -- 火攻buff
                    if buff[1] == FIRE_BUFF then 
                        is_end, tip2, die_num2 = skill.fire(hero, hero_list, buff) 
                    end
                    -- 洪水buff
                    if buff[1] == FLOOD_BUFF then 
                        is_end, tip2, die_num2 = skill.flood(hero, hero_list, buff) 
                    end
                    -- 风暴buff
                    if buff[1] == STORM_BUFF then 
                        is_end, tip2, die_num2 = skill.storm(hero, hero_list, buff) 
                    end
                    -- 回血buff
                    if buff[1] == RECOVER_BUFF then 
                        is_end, tip2, die_num2 = skill.recover(hero, hero_list, buff) 
                    end
                    
                    -- 死亡处理
                    if is_end == true then return is_end, 0, 0 end
                    tip = tip + tip2
                    die_num = die_num + die_num2
                    --print(is_end, tip, die_num)
                end
            end
        end
    end

    return is_end, tip, die_num
end

-- 普攻触发
function normal_attack_tri(hero, hero_list)
    local is_end, tip, die_num = false, 0, 0
-- 普攻触发
    for a=1, max_skills_num, 1 do

        local result = is_tri_time(hero, NORMAL_ATTACK_TIME, a)
        if result == true then
            local skill_id = hero.skills[a]
            local buff_list = Skill[skill_id].attribute
            local tip2, die_num2 = 0, 0

            -- 循环处理每个技能触发buff
            for i, buff in pairs(buff_list) do
                local result2 = is_can_tri(buff)
                if result2 == true then
                    -- buff埋点
                    -- 普通普攻buff
                    if buff[1] == NORMAL_ATTACK_BUFF then 
                        is_end, tip2, die_num2 = skill.normal_atk(hero, hero_list, buff) 
                    end

                    -- 死亡处理
                    if is_end == true then return is_end, 0, 0 end
                    tip = tip + tip2
                    die_num = die_num + die_num2
                    --print(is_end, tip, die_num)
                end
            end
        end
    end

    return is_end, tip, die_num
end

-- 追击触发
function atk_normal_tri(hero, hero_list)
    local is_end, tip, die_num = false, 0, 0

    for a=1, max_skills_num, 1 do

        local result = is_tri_time(hero, ATTACK_NORMAL_TIME, a)
        if result == true then
            local skill_id = hero.skills[a]
            local buff_list = Skill[skill_id].attribute
            local tip2, die_num2 = 0, 0

            -- 循环处理每个技能触发buff
            for i, buff in pairs(buff_list) do
                local result2 = is_can_tri(buff)
                if result2 == true then
                    -- buff埋点
                    -- 兵刃直伤buff
                    if buff[1] == PHY_HURT_BUFF then 
                        is_end, tip2, die_num2 = skill.phy_hurt(hero, hero_list, buff) 
                    end
                    -- 谋略直伤buff
                    if buff[1] == MAG_HURT_BUFF then 
                        is_end, tip2, die_num2 = skill.mag_hurt(hero, hero_list, buff) 
                    end
                    -- 火攻buff
                    if buff[1] == FIRE_BUFF then 
                        is_end, tip2, die_num2 = skill.fire(hero, hero_list, buff) 
                    end
                    -- 洪水buff
                    if buff[1] == FLOOD_BUFF then 
                        is_end, tip2, die_num2 = skill.flood(hero, hero_list, buff) 
                    end
                    -- 风暴buff
                    if buff[1] == STORM_BUFF then 
                        is_end, tip2, die_num2 = skill.storm(hero, hero_list, buff) 
                    end
                    -- 回血buff
                    if buff[1] == RECOVER_BUFF then 
                        is_end, tip2, die_num2 = skill.recover(hero, hero_list, buff) 
                    end
                    
                    -- 死亡处理
                    if is_end == true then return is_end, 0, 0 end
                    tip = tip + tip2
                    die_num = die_num + die_num2
                    --print(is_end, tip, die_num)
                end
            end
        end
    end

    return is_end, tip, die_num
    
end

function round_end_tri()
    
end

function buff_recycle(hero, hero_list)
    local a = 1
    local total = #hero.buff

    while a <= total do
        if Buff[hero.buff[a].buff_id].is_continous == CONTINOUS then

            --print(hero.name, Buff[hero.buff[a].buff_id].buff_name, hero.buff[a].continue_round - 1)

            hero.buff[a].continue_round = hero.buff[a].continue_round - 1
            if hero.buff[a].continue_round <= 0 then 
                -- 火攻
                if hero.buff[a].buff_id == FIRE_BUFF then
                    hero.atk_mag = hero.atk_mag + hero.buff[a].hurt_mult
                end
                -- 洪水
                if hero.buff[a].buff_id == FLOOD_BUFF then
                    hero.atk_mag = hero.def + hero.buff[a].hurt_mult
                end
                -- 风暴
                if hero.buff[a].buff_id == STORM_BUFF then
                    hero.atk_mag = hero.speed + hero.buff[a].hurt_mult
                end

                -- print(hero.name, Buff[hero.buff[a].buff_id].buff_name, "buff消失")
                Logger.log(LOG_TYPE_6, hero.name, Buff[hero.buff[a].buff_id].buff_name)
                table.remove(hero.buff, a)
                a = a - 1
                total = total - 1
            end
        end
        a = a + 1
    end

    return false, 0, 0
end


-- 判断随机概率
function random_bool(ran_num)
    local result = math.random(1, 100)

    if result > ran_num then return false end
    return true
end


-- 初始化角色数值
function init_role(rolea, roleb)
    local uid = 0 --武将局内唯一id

    for i, hero in ipairs(rolea.heros) do
        hero.name = Hero[hero.heroid].name
        hero.hp = Hero[hero.heroid].hp
        hero.atk_phy = Hero[hero.heroid].atk_phy
        hero.atk_mag = Hero[hero.heroid].atk_mag
        hero.def = Hero[hero.heroid].def
        hero.speed = Hero[hero.heroid].speed
        hero.buff = {}
        hero.belong = 1 -- 玩家1
        hero.uid = uid + 1 -- 唯一id
        uid = uid + 1
        hero.crit = 0 -- 百分比会心暴击
        hero.crit_hurt = 50 -- 百分比会心伤害倍率
        hero.rise = 0 -- 百分比增伤
        hero.reduce = 0 -- 百分比减伤 (封顶一百)
    end

    for i, hero in ipairs(roleb.heros) do
        hero.name = Hero[hero.heroid].name
        hero.hp = Hero[hero.heroid].hp
        hero.atk_phy = Hero[hero.heroid].atk_phy 
        hero.atk_mag = Hero[hero.heroid].atk_mag
        hero.def = Hero[hero.heroid].def
        hero.speed = Hero[hero.heroid].speed
        hero.buff = {}
        hero.belong = 2 -- 玩家2
        hero.uid = uid + 1 -- 唯一id
        uid = uid + 1
        hero.crit = 0 -- 会心暴击
        hero.crit_hurt = 50 -- 百分比会心伤害倍率
        hero.rise = 0 -- 百分比增伤
        hero.reduce = 0 -- 百分比减伤 (封顶一百)
    end

    return rolea, roleb
end


-- 是否到触发时机+发动概率判断
function is_tri_time(hero, tri_time, num)

    local skill_id = hero.skills[num]

    if skill_id ~= 0 then
        for i, time in ipairs(Skill[skill_id].tri_time) do
           if time[1] == tri_time and not check_debuff(hero, Skill[skill_id].skill_type) then 
                local random_num = math.random(1, 100)
                if random_num > time[2] then 
                    -- print(hero.name, "因几率没有触发主动技能", Skill[skill_id].skill_name)
                    Logger.log(LOG_TYPE_4, hero.name, Skill[skill_id].skill_name)
                    return false 
                end
                -- print(hero.name, "成功触发主动技能", Skill[skill_id].skill_name)
                Logger.log(LOG_TYPE_5, hero.name, Skill[skill_id].skill_name)
                return true 
            end
        end
    end

    return false
end

-- 判断有无负面
function check_debuff(hero, skill_type)
    local result

    if skill_type == SKILL_TYPE_ACTIVE then
        result = is_buff(hero, KILL_ACTIVE_BUFF)
        if result == true then 
            -- print(hero.name, "由于技穷状态无法发动主动技能")
            Logger.log(LOG_TYPE_13, hero.name)
            return true
        end
        
    elseif skill_type == SKILL_TYPE_NOR_ATK then
        result = is_buff(hero, DISARM_BUFF)
        if result == true then 
            -- print(hero.name, "由于缴械状态无法发动普攻")
            Logger.log(LOG_TYPE_14, hero.name)
            return true
        end
    end

    return false
end

-- 概率是否满足触发
function is_can_tri(buff) 
    local random_num = math.random(1, 100)

    if random_num > buff[2] then
        return false
    end

    return true
end


-- 获取当前文件名
function get_current_filename()
    local source = debug.getinfo(2, "S").source
    -- source 的格式通常为 "@文件路径"，需要提取文件名部分
    local filename = source:match("@(.+)$")
    return filename
end

return fight
