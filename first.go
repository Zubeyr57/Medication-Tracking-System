package main
import ("net/http")
func main(){
	http.HandleFunc("/",HelloHandler)
	http.ListenAndServe(":8888",nil)
}

func HelloHandler(res http.ResponseWriter, req *http.Request){
	res.Write([]byte("hello, World!"))
}