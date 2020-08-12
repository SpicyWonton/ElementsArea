package db

import (
	"database/sql"
	"log"

	_ "github.com/go-sql-driver/mysql"
)

type Mgtdb struct {
	db *sql.DB
}

//提供一个全局的db
var DbObj *Mgtdb

//默认加载init
func init() {
	DbObj = &Mgtdb{db: nil}
	var err error
	DbObj.db, err = sql.Open("mysql", "root:123456@/g2_server?charset=utf8")
	if err != nil {
		log.Println("sql open", err)
	}
}

//注册账号
func (mb *Mgtdb) Register(uid string, password string, Name string, img int32) bool {
	//在数据库里查询该账号是否已经被注册了
	tempid := 0
	err := mb.db.QueryRow("SELECT * FROM user where id = ?", uid).Scan(&tempid)

	if err == sql.ErrNoRows { //该账号未被注册过

		//插入user表
		_, err := mb.db.Exec("insert into user(id,password) values(?,?)", uid, password)
		if err != nil {
			log.Println("Register user err:", err)
			return false
		}

		//插入player表
		_, err = mb.db.Exec("insert into player(id,name,image) values(?,?,?)", uid, Name, img)
		if err != nil {
			log.Println("insert player err:", err)
			return false
		}

		return true
	} else {
		//该账号已经被注册
		return false
	}
}

//登录账户
func (mb *Mgtdb) LoginCheck(uid string, password string) bool {
	var tempid int32
	var temppw string
	err := mb.db.QueryRow("SELECT * FROM user where id = ? and Password = ?", uid, password).Scan(&tempid, &temppw)

	if err == sql.ErrNoRows { //判断是否有该数据
		log.Println("sql query", err)
		return false
	}
	return true
}

//获取玩家的基本信息
func (mb *Mgtdb) GetPlayerMsg(uid string) (name string, rank int32, experience int32, achievement int32, money int32, img int32, err error) {

	tempid := 0
	err = mb.db.QueryRow("SELECT * FROM player where id = ?", uid).Scan(&tempid, &name, &rank, &experience, &achievement, &money, &img)
	if err == sql.ErrNoRows {
		log.Println("sql query", err)
	}
	return
}

//更新玩家经验和等级
func (mb *Mgtdb) UpdatePlayer(uid string, exp int32, grade int32) {

	_, err := mb.db.Exec("update player set grade = ?, experience = ? where id = ?", grade, exp, uid)
	if err != nil {
		log.Println("UpdatePlayer player err:", err)
	}
}

//获得玩家游戏数据(总场数、总得分等)
func (mb *Mgtdb) getGameData(uid int32) (times, totalscore, maxsocre int32) {
	tempid := 0
	err := mb.db.QueryRow("SELECT * FROM record where id = ?", uid).Scan(&tempid, &times, &totalscore, &maxsocre)
	if err == sql.ErrNoRows { //判断是否有该数据
		log.Println("sql query", err)
	}
	return
}
