local fight = require("fight.main")
local players = require("config.playercf") 


-- 路径拓展
local function MakeLoad()
    -- 获取当前脚本所在路径
    local script_path = debug.getinfo(1, 'S').source:match("^@(.+/)") or ''

    -- 添加配置表文件夹的路径到 package.path
    package.path = package.path .. ';' .. script_path .. 'LuaXlsx/?.lua'
end


local function test()
    require("fight.ce")
end



-- 程序入口
local function main()
    -- 路径加载
    MakeLoad()
    math.randomseed(os.time())

    -- 开始战斗
    fight.main(players.player_a, players.player_b)
end


main()


