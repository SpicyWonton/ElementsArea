package inet

//路由接口
type IRouter interface{
	Deal(request IReq) //处理conn业务的方法
}