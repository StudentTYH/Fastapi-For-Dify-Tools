from fastapi.middleware.cors import CORSMiddleware
import os
from dotenv import load_dotenv
from paddleocr import PaddleOCR
import numpy as np
from PIL import Image
import base64
from io import BytesIO
from fastapi import FastAPI, File, UploadFile
from fastapi.staticfiles import StaticFiles
from pathlib import Path
import shutil
import aiohttp
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


# 定义存储目录
UPLOAD_DIR = Path("static/uploads")
UPLOAD_DIR.mkdir(parents=True, exist_ok=True)

# 挂载 `static` 目录，使其可通过 URL 访问
app.mount("/static", StaticFiles(directory="static"), name="static")

@app.post("/upload/")
async def upload_image(file: UploadFile = File(...)):
    # 生成文件保存路径
    file_path = UPLOAD_DIR / file.filename

    # 将上传的文件保存到 `static/uploads/` 目录
    with file_path.open("wb") as buffer:
        shutil.copyfileobj(file.file, buffer)
        buffer.close()
    # 返回可访问的 URL
    file_url = f"/static/uploads/{file.filename}"
    return {"filename": file.filename, "url": file_url}

async def ocr_tools(base64_string: str):

    img_data = base64.b64decode(base64_string)
    img = Image.open(BytesIO(img_data))
    image = np.array(img)


    det_model_dir = "./inference/ch_ppocr_server_v2.0_det_infer/"
    rec_model_dir = "./inference/ch_ppocr_server_v2.0_rec_infer/"
    cls_model_dir = "./inference/ch_ppocr_mobile_v2.0_cls_infer/"

    class OCR(PaddleOCR):
        def __init__(self, **kwargs):
            super(OCR, self).__init__(**kwargs)

        def ocr_new(self, img, det=True, rec=True, cls=True):
            res = self.ocr(img, det=det, rec=rec, cls=cls)
            if res!=None:
                return res
            else:
                img_v = Image.open(img).convert("RGB")
                img_v = np.asanyarray(img_v)[:,:,[2,1,0]]
                dt_boxes, rec_res = self.__call__(img_v)
                return [[box.tolist(), res] for box, res in zip(dt_boxes, rec_res)]


    paddle_ocr_engin = OCR(det_model_dir= det_model_dir,rec_model_dir= rec_model_dir,cls_model_dir= cls_model_dir,use_angle_cls=True)
    result = paddle_ocr_engin.ocr(image)


    all_text = ""
    if result==[None]:
        return all_text
    else:
        for idx in range(len(result)):
            res = result[idx]
            for line in res:
                all_text+=line[-1][0]
        return all_text


@app.post("/ocr")
async def ocr(url: str):
    async with aiohttp.ClientSession() as session:
        async with session.get(url) as response:
            if response.status != 200:
                return {"result": "图片读取失败"}
            img_bytes = await response.read()  # 读取图片内容

    img = base64.b64encode(img_bytes).decode('utf-8')

    ocr_text=await ocr_tools(img)
    return {"result": ocr_text}


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
