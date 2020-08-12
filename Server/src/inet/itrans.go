package inet

type ITrans interface {
	Reader(c *IConnection)
	Writer(c *IConnection)
}