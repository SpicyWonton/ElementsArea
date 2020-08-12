package GameCore

type PlayerMsg struct {
	Name string			//玩家昵称
	Rank int32			//玩家等级
	Experience int32	//玩家经验
	Achievement int32	//玩家成就点数
	HeadIcon int32		//玩家头像 0 - 3

	Score int32			//单局游戏得分
	GameTimes int32		//游戏的总次数
	TotalScore int32 	//游戏总得分
	MaxScore int32 		//单局最高得分
}

