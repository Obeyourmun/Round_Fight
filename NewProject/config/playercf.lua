local players = {}
local player_a = {}
local player_b = {}

local hero = require("LuaXlsx.Hero")

--{普攻id, 技能id1, 技能id2}
player_a.heros = {
    [1] = {heroid = 1, skills = {1, 11, 0}}, 
    [2] = {heroid = 2, skills = {1, 14, 0}}, 
    [3] = {heroid = 3, skills = {1, 13, 0}}
}
player_b.heros = {
    [1] = {heroid = 4, skills = {1, 12, 0}}, 
    [2] = {heroid = 5, skills = {1, 2, 0}}, 
    [3] = {heroid = 6, skills = {1, 3, 0}}
}

players.player_a = player_a
players.player_b = player_b

return players

