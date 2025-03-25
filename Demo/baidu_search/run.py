from fastapi.middleware.cors import CORSMiddleware
from fastapi import FastAPI
from bs4 import BeautifulSoup
import requests
import os
from dotenv import load_dotenv
load_dotenv(override=True)

app = FastAPI(servers=[
    {"url": os.getenv("YOUR_LOCAL_IP", "")}
])

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.post("/api/dify/baidu_search")
async def baidu_search(keyword : str):
    "通过搜索引擎查询指定关键词的相关资料"
    headers = {
		"User-Agent": "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36",
		"Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9",
		"Accept-Language": "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7",
		"Connection": "keep-alive",
		"Accept-Encoding": "gzip, deflate",
		"Host": "www.baidu.com",
		# 需要更换Cookie
        "Cookie":os.getenv("YOUR_WEB_COOKIE", "")
	}
    url="https://www.baidu.com/s?wd="+keyword
    response=requests.get(url,headers=headers)
    soup = BeautifulSoup(response.text)
    text=""
    for i in soup.find_all("div",class_="result c-container xpath-log new-pmd"):
        try:
            text=text+"tite:"+i.find("a").text+"\n"
            text=text+"content:"+i.find("span",class_="content-right_2s-H4").text+"\n"
        except:
            continue
    return { "result" : text}




if __name__ == "__main__":
    import uvicorn

    default_host = os.getenv("HOST", "0.0.0.0")
    default_port = int(os.getenv("FAST_API_PORT", "8767"))

    uvicorn.run(
        "run:app",
        host=default_host,
        port=default_port,
        reload=True
    )
