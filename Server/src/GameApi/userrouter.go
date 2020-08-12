package GameApi

import (
	"GameCore"
	"cnet"
	"db"
	"github.com/golang/protobuf/proto"
	"inet"
	"log"
	"math/rand"
	"pbf"
	"time"
)

/*
	玩家登陆-注册的业务
	以及成功登陆、注册后对进行游戏初始化（实例化对象，读取数据库数据）
 */


//登录的api
type LoginRouter struct {
	cnet.BaseRouter
}
func (*LoginRouter) Deal(req inet.IReq) {
	log.Println("[LoginRouter account]")

	msgdata := &pbf.SignIn{}
	err := proto.Unmarshal(req.GetData(), msgdata)
	if err != nil {
		log.Println("LoginRouter Unmarshal err:", err)
	}

	//验证账号和密码
	if db.DbObj.LoginCheck(msgdata.UID, msgdata.Password) {

		if v := GameCore.MgtplayerObj.GetPlayerById(msgdata.UID) ; v != nil {
			v.Conn.Stop()
			log.Println("该账号已经在线，强制下线之前的客户端")
		}

		log.Printf("登录成功 账号: %s \n", msgdata.UID)

		//从玩家表中读取玩家数据
		name, rank, experience, achievement, _,img, err := db.DbObj.GetPlayerMsg(msgdata.UID)

		//给客户端返回 账户-密码-Name-Rank-EXP
		msg, _ := proto.Marshal(&pbf.SignSucc{
			Flag:     true,
			UserInfo: &pbf.User{
				UID:      msgdata.UID,
				Password: msgdata.Password,
				Name:     name,
				Rank:     rank,
				EXP:      experience,
				HeadIcon: img,
			},
		})

		//发送给客户端
		err = req.GetConnection().SendMsg(uint32(pbf.MSGID_SIGNIN), msg)
		if err != nil {
			log.Println("LoginRouter SendMsg err:", err)
		}

		//设置连接的属性
		req.GetConnection().SetProperty("pid", msgdata.UID)

		//创建一个玩家实例
		player := &GameCore.Player{
			Id:   msgdata.UID,
			Conn: req.GetConnection(),
			Pmsg:    GameCore.PlayerMsg{
				Name:        name,
				Rank:		 rank,
				Experience:  experience,
				Achievement: achievement,
				HeadIcon:	 img,
			},
		}
		//添加玩家实例到mgtplayerobj中
		GameCore.MgtplayerObj.AddPlayer(player)

	} else {
		log.Printf("登录失败 账号 : %d 密码 : %s\n", msgdata.UID, msgdata.Password)

		msg, _ := proto.Marshal(&pbf.SignSucc{
			Flag:     false,
			UserInfo: &pbf.User{},
		})

		//发送给客户端
		err := req.GetConnection().SendMsg(1, msg)
		if err != nil {
			log.Println("LoginRouter SendMsg err:", err)
		}
	}
}

//注册的api
type registeredRouter struct {
	cnet.BaseRouter
}
func (*registeredRouter) Deal(req inet.IReq) {
	log.Println("[RegisterRouter account]")

	//反序列化取出数据
	msgdata := &pbf.SignUp{}
	err := proto.Unmarshal(req.GetData(), msgdata)
	if err != nil {
		log.Println("LoginRouter Unmarshal err:", err)
	}


	//产生一个随机数，用来分配头像
	rand.Seed(time.Now().Unix())
	x := int32(rand.Intn(4))

	//往数据库里插入数据
	res := db.DbObj.Register(msgdata.UID, msgdata.Password, msgdata.Name,x)

	//注册成功
	if res == true {
		//从玩家表中读取玩家数据
		//_, rank, experience, _, _, img, err := db.DbObj.GetPlayerMsg(msgdata.UID)
		msg, _ := proto.Marshal(&pbf.SignSucc{
			Flag:     true,
			UserInfo: &pbf.User{
				UID:      msgdata.UID,
				Password: msgdata.Password,
				Name:     msgdata.Name,
				Rank:     1,
				EXP:      0,
				HeadIcon: x,
			},
		})

		err = req.GetConnection().SendMsg(0, msg)
		if err != nil {
			log.Println("registeredRouter SendMsg err:", err)
		}

		//设置连接的属性
		req.GetConnection().SetProperty("pid", msgdata.UID)

		//创建一个玩家实例
		player := &GameCore.Player{
			Id:   msgdata.UID,
			Conn: req.GetConnection(),
			Pmsg:    GameCore.PlayerMsg{
				Name:         msgdata.Name,
				Rank:			1,
				Experience:  0,
				Achievement: 0,
				HeadIcon: x,
			},
		}
		//添加玩家实例到mgtplayerobj中
		GameCore.MgtplayerObj.AddPlayer(player)

	} else {
		msg, _ := proto.Marshal(&pbf.SignSucc{
			Flag:     false,
			UserInfo: &pbf.User{},
		})
		err = req.GetConnection().SendMsg(uint32(pbf.MSGID_SIGNUP), msg)
		if err != nil {
			log.Println("registeredRouter SendMsg err:", err)
		}
	}
}
