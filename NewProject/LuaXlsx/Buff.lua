-- buff_id                          int                              BuffId
-- buff_name                        string(trim)                     Buff名字
-- type                             int                              伤害类型[0没有伤害 1兵刃 2谋略, 3回血]
-- max_level                        int                              最大层数
-- effect                           string(trim)                     技能效果
-- is_continous                     int                              是否可持续:0不可 1可

return {
	[1] = {
		buff_id = 1,
		buff_name = "直接伤害[普攻]",
		type = 1,
		max_level = 1,
		effect = "",
		is_continous = 0,
	},
	[2] = {
		buff_id = 2,
		buff_name = "兵刃直伤",
		type = 1,
		max_level = 1,
		effect = "",
		is_continous = 0,
	},
	[3] = {
		buff_id = 3,
		buff_name = "谋略直伤",
		type = 2,
		max_level = 1,
		effect = "",
		is_continous = 0,
	},
	[4] = {
		buff_id = 4,
		buff_name = "火攻",
		type = 0,
		max_level = 1,
		effect = "智力下降",
		is_continous = 1,
	},
	[5] = {
		buff_id = 5,
		buff_name = "洪水",
		type = 0,
		max_level = 1,
		effect = "统帅下降",
		is_continous = 1,
	},
	[6] = {
		buff_id = 6,
		buff_name = "风暴",
		type = 0,
		max_level = 1,
		effect = "速度下降",
		is_continous = 1,
	},
	[7] = {
		buff_id = 7,
		buff_name = "回血",
		type = 3,
		max_level = 1,
		effect = "回血",
		is_continous = 0,
	},
	[8] = {
		buff_id = 8,
		buff_name = "技穷",
		type = 0,
		max_level = 1,
		effect = "不能放主动技能",
		is_continous = 1,
	},
	[9] = {
		buff_id = 9,
		buff_name = "断粮",
		type = 0,
		max_level = 1,
		effect = "回血效果只生效三成",
		is_continous = 1,
	},
	[10] = {
		buff_id = 10,
		buff_name = "虚弱",
		type = 0,
		max_level = 1,
		effect = "造成伤害削弱为三成",
		is_continous = 1,
	},
	[11] = {
		buff_id = 11,
		buff_name = "缴械",
		type = 0,
		max_level = 1,
		effect = "不能普攻",
		is_continous = 1,
	},
}
