local logger = {}

-- 日志编号
local HURT_TYPE_PHY = 1
local HURT_TYPE_MAG = 2
local HURT_TYPE_RECOVER = 3 -- 回血
local LOG_TYPE_4 = 4
local LOG_TYPE_5 = 5
local LOG_TYPE_6 = 6
local LOG_TYPE_7 = 7
local LOG_TYPE_8 = 8
local LOG_TYPE_9 = 9
local LOG_TYPE_10 = 10
local LOG_TYPE_11 = 11
local LOG_TYPE_12 = 12
local LOG_TYPE_13 = 13
local LOG_TYPE_14 = 14

local log_save = {}

function logger.log(type_log, ...)
    local args = {...}

    table.insert(log_save, {type_log, args})

end

function logger.log2()

    for k, v in ipairs(log_save) do
        local type_log = v[1]
        local args = v[2]

        if type_log == HURT_TYPE_RECOVER then
            -- print(hero.name, "recover", target.name, "recover_num:", hurt, target.name.."hp:", target.hp)
            print(args[1], "发动恢复能力", args[2], "回血数值:",  args[3], args[4].."hp:", args[5])
    
        elseif type_log == HURT_TYPE_PHY then
            -- print(hero.name, "hit", target.name, "hurt:", hurt, target.name.."hp:", target.hp)
            print(args[1], "对", args[2], "造成兵刃伤害", "伤害数值:", args[3], args[4].."hp:", args[5])
    
        elseif type_log == HURT_TYPE_MAG then
            -- print(hero.name, "hit", target.name, "hurt:", hurt, target.name.."hp:", target.hp)
            print(args[1], "对", args[2], "造成谋略伤害", "伤害数值:", args[3], args[4].."hp:", args[5])
    
        elseif type_log == LOG_TYPE_4 then
            -- print(hero.name, "因几率没有触发主动技能", Skill[skill_id].skill_name)
            print(args[1], "因几率没有触发主动技能", args[2])
        
        elseif type_log == LOG_TYPE_5 then
            -- print(hero.name, "成功触发主动技能", Skill[skill_id].skill_name)
            print(args[1], "成功触发技能", args[2])
        
        elseif type_log == LOG_TYPE_6 then
            -- print(hero.name, Buff[hero.buff[a].buff_id].buff_name, "buff消失")
            print(args[1], args[2], "buff消失")
    
        elseif type_log == LOG_TYPE_7 then
            print("game over")
    
        elseif type_log == LOG_TYPE_8 then
            -- print(hero.name, "挂上火攻状态", target.name, target.atk_mag, "减少智力", buff.hurt_mult, #target.buff)
            print(args[1], "给目标挂上火攻状态", args[2], args[3], "减少智力", args[4], args[5])
    
        elseif type_log == LOG_TYPE_9 then
            print(args[1], "给目标挂上洪水状态", args[2], args[3], "减少统帅", args[4])
    
        elseif type_log == LOG_TYPE_10 then
            print(args[1], "给目标挂上风暴状态", args[2], args[3], "减少统帅", args[4])
    
        elseif type_log == LOG_TYPE_11 then
            print(args[1].." die", "角色死亡")
    
        elseif type_log == LOG_TYPE_12 then
            print()
    
        elseif type_log == LOG_TYPE_13 then
            print(args[1], "由于技穷状态无法发动主动技能")
    
        elseif type_log == LOG_TYPE_14 then
            print(args[1], "由于缴械状态无法发动普攻")
    
        end
    end
    
end

return logger

