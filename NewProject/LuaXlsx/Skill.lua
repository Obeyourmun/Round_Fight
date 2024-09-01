-- skill_id                         int                              技能ID
-- skill_name                       string(trim)                     技能名字
-- attribute                        json                             技能属性[[1buffid,2概率百分比,3伤害倍率百分比or数值,4目标选取,5持续回合 6额外参数]]
-- tri_time                         json                             触发时机[[触发时刻，几率]] 1回合开始 2行动前 3主动 4普攻 5追击 6回合结束
-- skill_type                       int                              技能类型 (1指挥 2主动 3普攻 4追击 5被动)
-- desc                             string(trim)                     技能描述

return {
	[1] = {
		skill_id = 1,
		skill_name = "普攻",
		attribute = {
			[1] = {
				[1] = 1,
				[2] = 100,
				[3] = 100,
				[4] = 1,
			},
		},
		tri_time = {
			[1] = {
				[1] = 4,
				[2] = 100,
			},
		},
		skill_type = 3,
		desc = "",
	},
	[2] = {
		skill_id = 2,
		skill_name = "单体兵刃测试",
		attribute = {
			[1] = {
				[1] = 2,
				[2] = 100,
				[3] = 400,
				[4] = 1,
			},
		},
		tri_time = {
			[1] = {
				[1] = 3,
				[2] = 100,
			},
		},
		skill_type = 2,
		desc = "",
	},
	[3] = {
		skill_id = 3,
		skill_name = "随机二人兵刃测试",
		attribute = {
			[1] = {
				[1] = 2,
				[2] = 100,
				[3] = 200,
				[4] = 2,
			},
		},
		tri_time = {
			[1] = {
				[1] = 3,
				[2] = 100,
			},
		},
		skill_type = 2,
		desc = "",
	},
	[4] = {
		skill_id = 4,
		skill_name = "全体兵刃测试",
		attribute = {
			[1] = {
				[1] = 2,
				[2] = 100,
				[3] = 100,
				[4] = 3,
			},
		},
		tri_time = {
			[1] = {
				[1] = 3,
				[2] = 100,
			},
		},
		skill_type = 2,
		desc = "",
	},
	[5] = {
		skill_id = 5,
		skill_name = "单体谋略测试",
		attribute = {
			[1] = {
				[1] = 3,
				[2] = 100,
				[3] = 100,
				[4] = 1,
			},
		},
		tri_time = {
			[1] = {
				[1] = 3,
				[2] = 100,
			},
		},
		skill_type = 2,
		desc = "",
	},
	[6] = {
		skill_id = 6,
		skill_name = "随机二人谋略测试",
		attribute = {
			[1] = {
				[1] = 3,
				[2] = 100,
				[3] = 100,
				[4] = 2,
			},
		},
		tri_time = {
			[1] = {
				[1] = 3,
				[2] = 100,
			},
		},
		skill_type = 2,
		desc = "",
	},
	[7] = {
		skill_id = 7,
		skill_name = "全体谋略测试",
		attribute = {
			[1] = {
				[1] = 3,
				[2] = 100,
				[3] = 100,
				[4] = 3,
			},
		},
		tri_time = {
			[1] = {
				[1] = 3,
				[2] = 100,
			},
		},
		skill_type = 2,
		desc = "",
	},
	[8] = {
		skill_id = 8,
		skill_name = "火攻测试",
		attribute = {
			[1] = {
				[1] = 4,
				[2] = 100,
				[3] = 20,
				[4] = 1,
				[5] = 2,
			},
		},
		tri_time = {
			[1] = {
				[1] = 3,
				[2] = 50,
			},
		},
		skill_type = 2,
		desc = "",
	},
	[9] = {
		skill_id = 9,
		skill_name = "洪水测试",
		attribute = {
			[1] = {
				[1] = 5,
				[2] = 100,
				[3] = 20,
				[4] = 1,
				[5] = 2,
			},
		},
		tri_time = {
			[1] = {
				[1] = 3,
				[2] = 50,
			},
		},
		skill_type = 2,
		desc = "",
	},
	[10] = {
		skill_id = 10,
		skill_name = "风暴测试",
		attribute = {
			[1] = {
				[1] = 6,
				[2] = 100,
				[3] = 20,
				[4] = 2,
				[5] = 2,
			},
		},
		tri_time = {
			[1] = {
				[1] = 3,
				[2] = 50,
			},
		},
		skill_type = 2,
		desc = "",
	},
	[11] = {
		skill_id = 11,
		skill_name = "清风驱疾",
		attribute = {
			[1] = {
				[1] = 7,
				[2] = 100,
				[3] = 20,
				[4] = 5,
			},
		},
		tri_time = {
			[1] = {
				[1] = 3,
				[2] = 50,
			},
		},
		skill_type = 2,
		desc = "为随机两名友方回复体力",
	},
	[12] = {
		skill_id = 12,
		skill_name = "攻其不备",
		attribute = {
			[1] = {
				[1] = 2,
				[2] = 100,
				[3] = 280,
				[4] = 1,
				[5] = 0,
				[6] = {
					[1] = 8,
					[2] = 100,
					[3] = 0,
					[4] = 0,
					[5] = 1,
				},
			},
		},
		tri_time = {
			[1] = {
				[1] = 5,
				[2] = 50,
			},
		},
		skill_type = 4,
		desc = "普攻后对当前敌方造成一定兵刃伤害并技穷",
	},
	[13] = {
		skill_id = 13,
		skill_name = "决水破敌",
		attribute = {
			[1] = {
				[1] = 2,
				[2] = 100,
				[3] = 150,
				[4] = 2,
			},
			[2] = {
				[1] = 5,
				[2] = 80,
				[3] = 20,
				[4] = 1,
				[5] = 2,
			},
		},
		tri_time = {
			[1] = {
				[1] = 3,
				[2] = 50,
			},
		},
		skill_type = 2,
		desc = "对随机两名敌方造成一定伤害并一定概率释放洪水buff持续两回合",
	},
	[14] = {
		skill_id = 14,
		skill_name = "火羽",
		attribute = {
			[1] = {
				[1] = 2,
				[2] = 100,
				[3] = 150,
				[4] = 3,
			},
			[2] = {
				[1] = 4,
				[2] = 100,
				[3] = 20,
				[4] = 3,
				[5] = 2,
			},
		},
		tri_time = {
			[1] = {
				[1] = 3,
				[2] = 50,
			},
		},
		skill_type = 2,
		desc = "对敌方全体造成一定伤害并施加火攻buff,持续两回合",
	},
}
