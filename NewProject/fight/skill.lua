local Hero = require("LuaXlsx.Hero")
local Skill = require("LuaXlsx.Skill")
local Buff = require("LuaXlsx.Buff")
local Logger = require("fight.log")

local skill = {}

-- 常量声明
-- buff列表
local NORMAL_ATTACK_BUFF = 1 -- 普通攻击 
local PHY_HURT_BUFF = 2 -- 兵刃直伤
local MAG_HURT_BUFF = 3 -- 谋略直伤
local FIRE_BUFF = 4 -- 火攻
local FLOOD_BUFF = 5 -- 洪水
local STORM_BUFF = 6 -- 风暴
local RECOVER_BUFF = 7 -- 回血效果
local KILL_ACTIVE_BUFF = 8 -- 技穷
local KILL_GRAIN_BUFF = 9 -- 断粮
local WEAK_BUFF = 10 -- 虚弱
local DISARM_BUFF = 11 -- 缴械


-- 目标选取
local RANDOM_ONE = 1 -- 敌军随机一人
local RANDOM_TWO = 2 -- 敌军随机两人
local ALL_ENEMY = 3 -- 敌军全体
local RANDOM_ONE_MY = 4 -- 我军随机一人
local RANDOM_TWO_MY = 5 -- 我军随机两人
local ALL_ENEMY_MY = 6 -- 我军全体
local TAR_SELF = 7 -- 自身

-- 触发时机
local ROUND_BEGIN_TIME = 1 -- 回合开始时
local RUN_BEGIN_TIME = 2 -- 行动前
local ACTIVE_TIME = 3 -- 主动技
local NORMAL_ATTACK_TIME = 4 -- 普攻
local ATTACK_NORMAL_TIME = 5 -- 追击(普攻后)
local ROUND_END_TIME = 6 -- 回合结束时

-- 伤害类型
local HURT_TYPE_PHY = 1
local HURT_TYPE_MAG = 2
local HURT_TYPE_RECOVER = 3 -- 回血

-- LOG
local LOG_TYPE_8 = 8
local LOG_TYPE_9 = 9
local LOG_TYPE_10 = 10
local LOG_TYPE_11 = 11

local NOT_CONTINOUS = 0 -- 不持续的buff 
local CONTINOUS = 1 -- 持续的buff 



-- 普通普攻
function skill.normal_atk(hero, hero_list, buff)
    local target_list = get_target(buff[4], hero, hero_list)
    local result = put_buff(buff, hero, hero_list, target_list)
    if result == false then fslove() return false, 0, 0 end

    local is_end, tip, die_num = tri_buff(NORMAL_ATTACK_BUFF, hero, target_list, hero_list)

    -- 游戏结束
    if is_end == true then return is_end, tip, die_num end

    return is_end, tip, die_num
end

-- 兵刃直伤
function skill.phy_hurt(hero, hero_list, buff)
    local target_list = get_target(buff[4], hero, hero_list)
    local result = put_buff(buff, hero, hero_list, target_list)
    if result == false then fslove() return false, 0, 0 end

    local is_end, tip, die_num = tri_buff(PHY_HURT_BUFF, hero, target_list, hero_list)

    -- 游戏结束
    if is_end == true then return is_end, tip, die_num end

    -- 附带buff
    local buff2, target_list2
    if buff[6] then 
        buff2 = buff[6]
        if buff2[4] == 0 then target_list2 = target_list end
        
        local result = put_buff(buff2, hero, hero_list, target_list)
        if result == false then fslove() return false, 0, 0 end

        local is_end, tip, die_num = tri_buff(buff2[1], hero, target_list, hero_list)
    end

    -- 游戏结束
    if is_end == true then return is_end, tip, die_num end



    return is_end, tip, die_num
end

-- 谋略直伤
function skill.mag_hurt(hero, hero_list, buff)
    local target_list = get_target(buff[4], hero, hero_list)
    local result = put_buff(buff, hero, hero_list, target_list)
    if result == false then fslove() return false, 0, 0 end

    local is_end, tip, die_num = tri_buff(MAG_HURT_BUFF, hero, target_list, hero_list)

    -- 游戏结束
    if is_end == true then return is_end, tip, die_num end

    return is_end, tip, die_num
end

-- 回血
function skill.recover(hero, hero_list, buff)
    local target_list = get_target(buff[4], hero, hero_list)
    local result = put_buff(buff, hero, hero_list, target_list)
    if result == false then fslove() return false, 0, 0 end

    local is_end, tip, die_num = tri_buff(RECOVER_BUFF, hero, target_list, hero_list)

    -- 游戏结束
    if is_end == true then return is_end, tip, die_num end

    return is_end, tip, die_num
end

-- 火攻
function skill.fire(hero, hero_list, buff)
    local is_end, tip, die_num = false, 0 ,0
    
    local target_list = get_target(buff[4], hero, hero_list)
    local target_list2 = {}

    for k, v in ipairs(target_list) do table.insert(target_list2, v) end
    local a = 1
    local total = #target_list2
    while a <= total do
        if is_buff(target_list2[a], FIRE_BUFF) == true then
            table.remove(target_list2, a)
            a = a - 1
            total = total - 1
        end

        a = a + 1
    end

    local result = put_buff(buff, hero, hero_list, target_list)
    if result == false then fslove() return false, 0, 0 end

    is_end, tip, die_num = tri_buff(FIRE_BUFF, hero, target_list2, hero_list)

    -- 游戏结束
    if is_end == true then return is_end, tip, die_num end

    return is_end, tip, die_num
end

-- 洪水
function skill.flood(hero, hero_list, buff)
    local is_end, tip, die_num = false, 0 ,0
    
    local target_list = get_target(buff[4], hero, hero_list)
    local target_list2 = {}

    for k, v in ipairs(target_list) do table.insert(target_list2, v) end
    local a = 1
    local total = #target_list2
    while a <= total do
        if is_buff(target_list2[a], FLOOD_BUFF) == true then
            table.remove(target_list2, a)
            a = a - 1
            total = total - 1
        end

        a = a + 1
    end

    local result = put_buff(buff, hero, hero_list, target_list)
    if result == false then fslove() return false, 0, 0 end

    is_end, tip, die_num = tri_buff(FLOOD_BUFF, hero, target_list2, hero_list)

    -- 游戏结束
    if is_end == true then return is_end, tip, die_num end

    return is_end, tip, die_num
end

-- 风暴
function skill.storm(hero, hero_list, buff)
    local is_end, tip, die_num = false, 0 ,0
    
    local target_list = get_target(buff[4], hero, hero_list)
    local target_list2 = {}

    for k, v in ipairs(target_list) do table.insert(target_list2, v) end
    local a = 1
    local total = #target_list2
    while a <= total do
        if is_buff(target_list2[a], STORM_BUFF) == true then
            table.remove(target_list2, a)
            a = a - 1
            total = total - 1
        end

        a = a + 1
    end

    local result = put_buff(buff, hero, hero_list, target_list)
    if result == false then fslove() return false, 0, 0 end

    is_end, tip, die_num = tri_buff(STORM_BUFF, hero, target_list2, hero_list)

    -- 游戏结束
    if is_end == true then return is_end, tip, die_num end

    return is_end, tip, die_num
end


-- 上buff
function put_buff(buff, hero, hero_list, target_list)

    if target_list == false then return false end

    -- buff实装
    for i, be_atker in ipairs(target_list) do
    
        local role_buff = make_buff(buff, hero)
        --print(hero.name, Buff[buff[1]].buff_name, buff[5])
        if buff[5] and buff[5] ~= 0 then role_buff.continue_round = buff[5] end -- 持续回合

        -- 是否叠层
        local is_put = 0
        for k, v in pairs(be_atker.buff) do
            if v.buff_id == role_buff.buff_id then
                is_put = 1
                v.level = v.level + 1
                if buff[5] then v.continue_round = buff[5] end
                if v.level > Buff[v.buff_id].max_level then
                    v.level = Buff[v.buff_id].max_level
                end
            end
        end

        -- if role_buff.buff_id == 4 then print("火攻落", is_put) end
        -- if role_buff.buff_id == 5 then print("洪水落", is_put) end
        -- if role_buff.buff_id == 6 then print("风暴落", is_put, be_atker.name, i) end

        if is_put == 0 then table.insert(be_atker.buff, role_buff) end      
    end

    return true
end
 

-- buff触发   叠层buff未处理
function tri_buff(buff_id, hero, target_list, hero_list)
    local is_end, tip, die_num = false, 0, 0
    local tip2, die_num2 = 0, 0

    for i, target in pairs(target_list) do
        for it, v in pairs(target.buff) do
            if target.buff[it].buff_id == buff_id then
                -- 普通攻击
                if buff_id == NORMAL_ATTACK_BUFF then
                    is_end, tip2, die_num2 = do_damage(hero, target, target.buff[it].hurt_mult/100, target.buff[it].hurt_type, hero_list)
                end
                -- 兵刃直伤
                if buff_id == PHY_HURT_BUFF then
                    is_end, tip2, die_num2 = do_damage(hero, target, target.buff[it].hurt_mult/100, target.buff[it].hurt_type, hero_list)
                end
                -- 谋略直伤
                if buff_id == MAG_HURT_BUFF then
                    is_end, tip2, die_num2 = do_damage(hero, target, target.buff[it].hurt_mult/100, target.buff[it].hurt_type, hero_list)
                end
                -- 火攻
                if buff_id == FIRE_BUFF then
                    is_end, tip2, die_num2 = do_state(hero, target, target.buff[it], hero_list)
                end
                -- 洪水
                if buff_id == FLOOD_BUFF then
                    is_end, tip2, die_num2 = do_state(hero, target, target.buff[it], hero_list)
                end
                -- 风暴
                if buff_id == STORM_BUFF then
                    is_end, tip2, die_num2 = do_state(hero, target, target.buff[it], hero_list)
                end
                -- 回血
                if buff_id == RECOVER_BUFF then
                    is_end, tip2, die_num2 = do_damage(hero, target, target.buff[it].hurt_mult/100, target.buff[it].hurt_type, hero_list)
                end

                if is_end == true then return is_end, 0, 0 end
                -- 死亡处理
                tip = tip + tip2
                die_num = die_num + die_num2

                -- 移除buff 立即结算的buff
                if Buff[buff_id].is_continous == NOT_CONTINOUS then 
                    --print("移除通用")
                    table.remove(target.buff, it) 
                end
                break
            end
        end
    end

    return is_end, tip, die_num
end


-- 伤害落地 倍率全是1左右，传值去掉百分号
function do_damage(hero, target, hurt_mult, hurt_type, hero_list)

    local atk_rise = get_atk_rise(hero)
    local def_reduce = get_def_reduce(target, hurt_type)
    local hurt = 0

    if hurt_type == HURT_TYPE_RECOVER then 
        if not is_buff(target, KILL_GRAIN_BUFF) then
            hurt = 0 - hero.atk_mag*(hurt_mult)*(1+atk_rise)*(1-def_reduce)
        else
            hurt = 0 - hero.atk_mag*(hurt_mult)*(1+atk_rise)*(1-def_reduce) * 0.3
        end

        target.hp = target.hp - hurt
        if target.hp > 10000 then target.hp = 10000 end
        Logger.log(HURT_TYPE_RECOVER, hero.name, target.name, -hurt, target.name, target.hp)
        --print(hero.name, "recover", target.name, "recover_num:", hurt, target.name.."hp:", target.hp)
    elseif hurt_type == HURT_TYPE_PHY then
        if not is_buff(target, WEAK_BUFF) then
            hurt = hero.atk_phy*(hurt_mult)*(1+atk_rise)*(1-def_reduce)
        else
            hurt = hero.atk_phy*(hurt_mult)*(1+atk_rise)*(1-def_reduce) * 30
        end

        target.hp = target.hp - hurt
        Logger.log(HURT_TYPE_PHY, hero.name, target.name, hurt, target.name, target.hp)
        --print(hero.name, "hit", target.name, "hurt:", hurt, target.name.."hp:", target.hp)
    elseif hurt_type == HURT_TYPE_MAG then
        if not is_buff(target, WEAK_BUFF) then
            hurt = hero.atk_mag*(hurt_mult)*(1+atk_rise)*(1-def_reduce)
        else
            hurt = hero.atk_mag*(hurt_mult)*(1+atk_rise)*(1-def_reduce) * 0.3
        end

        target.hp = target.hp - hurt
        Logger.log(HURT_TYPE_PHY, hero.name, target.name, hurt, target.name, target.hp)
        --print(hero.name, "hit", target.name, "hurt:", hurt, target.name.."hp:", target.hp)
    end


    if target.hp > 10000 then target.hp = 10000 end
    if target.hp <= 0 then 
        local is_end, tip, die_num = die_solve(hero, target, hero_list)
        return is_end, tip, die_num -- 是否结束，外部循环下标，死亡人数
    end
    return false, 0, 0
end

-- 给人物上状态buff
function do_state(hero, target, buff, hero_list)
    local buff_id = buff.buff_id

    if buff_id == FIRE_BUFF then
        target.atk_mag = target.atk_mag - buff.hurt_mult
        -- print(hero.name, "挂上火攻状态", target.name, target.atk_mag, "减少智力", buff.hurt_mult, #target.buff)
        Logger.log(LOG_TYPE_8, hero.name, target.name, target.atk_mag, buff.hurt_mult, #target.buff)
    end
    if buff_id == FLOOD_BUFF then
        target.def = target.def - buff.hurt_mult
        -- print(hero.name, "挂上洪水状态", target.name, target.def, "减少统帅", buff.hurt_mult)
        Logger.log(LOG_TYPE_9, hero.name, target.name, target.def, buff.hurt_mult)
    end
    if buff_id == STORM_BUFF then
        target.speed = target.speed - buff.hurt_mult
        -- print(hero.name, "挂上风暴状态", target.name, target.speed, "减少速度", buff.hurt_mult)
        Logger.log(LOG_TYPE_10, hero.name, target.name, target.speed, buff.hurt_mult)
    end

    return false, 0, 0
end


-- 获取命中目标
function get_target(target_id, hero, hero_list)
    local enemy_list = {}
    local own_list = {}
    local own_tip = hero.belong

    for k, v in ipairs(hero_list) do
        if v.belong == own_tip then table.insert(own_list, v) 
        else table.insert(enemy_list, v) end
    end

    -- 敌军随机一人
    if target_id == RANDOM_ONE then
        local choose = math.random(1, #enemy_list)
        return {enemy_list[choose]}
    end
    -- 敌军随机两人
    if target_id == RANDOM_TWO then
        local temp = {}
        for k, v in ipairs(enemy_list) do
            table.insert(temp, k)
        end

        local choose = math.random(1, #temp)
        local one = temp[choose]
        table.remove(temp, choose)
        if #temp == 0 then return {enemy_list[choose]} end
        local choose2 = math.random(1, #temp)
        local two = temp[choose2]

        --print(enemy_list[one].name, enemy_list[two].name)
        return {enemy_list[one], enemy_list[two]}
    end
    -- 敌军全体
    if target_id == ALL_ENEMY then
        return enemy_list
    end
    -- 我军随机一人
    if target_id == RANDOM_ONE_MY then
        local choose = math.random(1, #own_list)
        return {own_list[choose]}
    end
    -- 我军随机两人
    if target_id == RANDOM_TWO_MY then
        local temp = {}
        for k, v in ipairs(own_list) do
            table.insert(temp, k)
        end

        local choose = math.random(1, #temp)
        local one = temp[choose]
        table.remove(temp, choose)
        if #temp == 0 then return {own_list[choose]} end
        local choose2 = math.random(1, #temp)
        local two = temp[choose2]

        --print(enemy_list[one].name, enemy_list[two].name)
        return {own_list[one], own_list[two]}
    end
    -- 我军全体
    if target_id == ALL_ENEMY_MY then
        return own_list
    end
    -- 自身
    if target_id == TAR_SELF then
        return {hero}
    end
    
    return {} -- 没找到
end

-- 封装buff
function make_buff(buff, hero)
    local role_buff = {
        buff_id = buff[1],
        puter = hero,  -- 攻击者
        hurt_mult = buff[3],  -- 伤害系数
        target_id = buff[4],  -- 目标选取类型
        hurt_type = Buff[buff[1]].type, -- 伤害类型
        level = 1, -- 层数
    }
    
    return role_buff
end

-- 获取减伤倍率
function get_def_reduce(target, hurt_type)
    local reduce_num = 0

    local phy_def = target.def

    if hurt_type == HURT_TYPE_PHY then
        reduce_num = reduce_num + phy_def/10
    else
        reduce_num = reduce_num + phy_def/20 + target.atk_mag/20
    end

    if reduce_num > 90 then reduce_num = 90 end -- 最高90减伤

    return reduce_num/100
end

-- 增伤倍率
function get_atk_rise(hero)
    local rise_num = 0

    -- 是否暴击
    local random_num = math.random(1, 100)
    if random_num <= hero.crit then rise_num = rise_num + hero.crit_hurt end

    return rise_num/100
end

-- 死亡处理
function die_solve(hero, target, hero_list)
    local tip, die_num = 0, 0
    if target.run_rank <= hero.run_rank then
        tip = tip + 1
        die_num = die_num + 1
    else
        die_num = die_num + 1
    end

    -- 移除武将指针
    table.remove(hero_list, target.run_rank)

    -- print(target.name.." die", "角色死亡")
    Logger.log(LOG_TYPE_11, target.name)

    -- 判断是否战斗结束
    local is_end = 1
    for i, v in ipairs(hero_list) do
        if v.belong == target.belong then
            is_end = 0
        end
    end
    if is_end == 1 then return true, tip, die_num end

    return false, tip, die_num -- 主循环下标和总值减少
end

-- 是否存在buff
function is_buff(hero, tar_buff_id)
    for k, v in ipairs(hero.buff) do
        if v.buff_id == tar_buff_id then return true end
    end

    return false
end

-- 函数错误路径
function fslove()
    -- 调用函数获取当前文件名并输出
    local current_filename = get_current_filename()
    -- 获取当前代码所在的行数
    local line_num = debug.getinfo(1, 'l').currentline

    print(current_filename, line_num)
end

-- 打印表格的函数
function printTable(tbl, indent)
    indent = indent or 0
    local indentStr = string.rep("  ", indent)
    
    -- 遍历表格的键值对
    for key, value in pairs(tbl) do
        if type(value) == "table" then
            -- 如果值是一个表格，递归打印
            print(indentStr .. tostring(key) .. ":")
            printTable(value, indent + 1)
        else
            -- 否则，打印键值对
            if type(key) == "number" then
                print(indentStr .. "[" .. key .. "] = " .. tostring(value))
            else
                print(indentStr .. tostring(key) .. " = " .. tostring(value))
            end
        end
    end
end

-- 示例
local myTable = {
    name = "Alice",
    age = 30,
    skills = {"Lua", "Python", "C++"},
    address = {
        street = "123 Main St",
        city = "Wonderland"
    }
}



return skill