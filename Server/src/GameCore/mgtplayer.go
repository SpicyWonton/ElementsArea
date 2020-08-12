package GameCore

import "sync"

type MgtPlayer struct {
	Players map[string]*Player	//玩家map key是uid  value是player实例
	playerLock sync.RWMutex		//保护玩家map的读写锁
}

//提供一个全局玩家管理实例
var MgtplayerObj *MgtPlayer

//提供初始化方法
func init() {
	MgtplayerObj = &MgtPlayer{Players: make(map[string]*Player)}
}

//添加一个玩家
func(mp *MgtPlayer) AddPlayer(player *Player) {
	//加写锁
	mp.playerLock.Lock()
	//添加玩家到map中
	mp.Players[player.Id] = player
	//解写锁
	mp.playerLock.Unlock()
}

//删除一个玩家
func(mp *MgtPlayer) DeletePlayer(uid string) {
	//加写锁
	mp.playerLock.Lock()
	//从map中删除玩家
	delete(mp.Players, uid)
	//解写锁
	mp.playerLock.Unlock()
}

//获取所有玩家
func(mp *MgtPlayer) GetAllPlayer() []*Player {
	players := make([]*Player, 0)
	//加读锁
	mp.playerLock.RLock()
	//遍历获得所有玩家实例
	for _, v := range mp.Players {
		players = append(players, v)
	}
	//解读锁
	mp.playerLock.RUnlock()
	return players
}

//通过id获取玩家
func(mp *MgtPlayer) GetPlayerById(uid string) *Player {
	//加读锁
	mp.playerLock.RLock()
	defer mp.playerLock.RUnlock()
	return mp.Players[uid]
}

//获取当前有多少在线的玩家
func(mp *MgtPlayer) GetNumPlayer() int {
	return len(mp.Players)
}

