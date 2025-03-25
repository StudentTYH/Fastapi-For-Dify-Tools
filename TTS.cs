using Microsoft.CognitiveServices.Speech;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;

namespace HumanVoice_Backstage.TTS
{
    public enum TTSType
    {
        HuoShan,
        keLong,
        Coze,
        Free
    }
    internal sealed class TTS
    {
        TTSType type = TTSType.HuoShan;

        string voiceType = "BV700_streaming";


        public TTS(string voiceType = null, TTSType type = TTSType.HuoShan)
        {
            this.type = type;
            if (!string.IsNullOrWhiteSpace(voiceType))
            {
                this.voiceType = voiceType;
            }
        }

        public byte[] Generate(string content)
        {
            if (content == "??")
                return null;

            switch (type) //根据 type 的不同值来调用不同的语音合成（TTS）方法
            {
                case TTSType.HuoShan:
                    return Syn(content);
                case TTSType.keLong:
                    return Syn_Kelong(content);
                case TTSType.Coze:
                    return Syn_Coze(content);
                    case TTSType.Free:
                    return Free(content);
                default:
                    return null;
            }
        }

        string appid = "4582700729";
        string token = "_F5mN4yyhlDp3e-9cSY0WCVJUnC3KZeO";

        static string host = "openspeech.bytedance.com";
        static string apiUrl = $"https://{host}/api/v1/tts";

        public byte[] Syn(string content)
        {
            var requestJson = new
            {
                app = new
                {
                    appid = appid,
                    token = token,
                    cluster = "volcano_tts"
                },
                user = new
                {
                    uid = "237845387534"
                },
                audio = new
                {
                    voice_type = voiceType,
                    encoding = "wav",
                    rate = "16000",
                },
                request = new
                {
                    reqid = Guid.NewGuid().ToString(),
                    text = content,
                    text_type = "plain",
                    operation = "query",
                    with_frontend = 1,
                    frontend_type = "unitTson"
                }
            };

            string requestBody = JsonConvert.SerializeObject(requestJson);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers["Authorization"] = $"Bearer;{token}";

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(requestBody);
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    string responseBody = streamReader.ReadToEnd();

                    var responseJson = JsonConvert.DeserializeObject<dynamic>(responseBody);
                    if (responseJson.data != null)
                    {
                        byte[] audioData = Convert.FromBase64String((string)responseJson.data);
                        return audioData;
                    }
                }
            }
            catch (WebException ex)
            {
                try
                {
                    using (var streamReader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        string errorResponse = streamReader.ReadToEnd();
                        Console.WriteLine($"Error response: \n{errorResponse}");
                    }
                }
                catch
                {

                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception caught: {e}");
            }

            return null;
        }









        //克隆
        static string appid_kelong = "4582700729";
        static string accessToken = "_F5mN4yyhlDp3e-9cSY0WCVJUnC3KZeO";
        static string cluster = "volcano_icl";
        static string host_kelong = "openspeech.bytedance.com";
        static string apiUrl_kelong = $"https://{host}/api/v1/tts";
        static string uid = "4334";
        public byte[] Syn_Kelong(string content)
        {
            var requestJson = new
            {
                app = new
                {
                    appid = appid,
                    token = accessToken,
                    cluster = cluster
                },
                user = new
                {
                    uid = uid
                },
                audio = new
                {
                    voice_type = voiceType,
                    encoding = "wav",
                    rate = "16000",
                    speed_ratio = 1.0,
                    volume_ratio = 1.0,
                    pitch_ratio = 1.0,
                },
                request = new
                {
                    reqid = Guid.NewGuid().ToString(),
                    text = content,
                    text_type = "plain",
                    operation = "query",
                    with_frontend = 1,
                    frontend_type = "unitTson"
                }
            };


            string requestBody = JsonConvert.SerializeObject(requestJson);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers["Authorization"] = $"Bearer;{accessToken}";

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(requestBody);
            }


            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    string responseBody = streamReader.ReadToEnd();

                    var responseJson = JsonConvert.DeserializeObject<dynamic>(responseBody);
                    if (responseJson.data != null)
                    {
                        byte[] audioData = Convert.FromBase64String((string)responseJson.data);
                        return audioData;

                    }
                }

            }

            catch (Exception e)
            {
                Console.WriteLine($"Exception caught: {e}");
            }

            return null;
        }











        //Coze
        static string cozeKey = "pat_gMdScPW1FpapQQ1T4bM53yx6qyg4O0n2sMZqDKoOZT9nJThd6vC4sdwSXJxysdAm";
        public byte[] Syn_Coze(string content)
        {
            var requestJson = new
            {
                input = content,
                voice_id = voiceType,
                response_format = "wav",
                sample_rate = 16000,
                speed = 1.1f
            };


            string requestBody = JsonConvert.SerializeObject(requestJson);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.coze.cn/v1/audio/speech");
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers["Authorization"] = $"Bearer {cozeKey}";

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(requestBody);
            }


            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using MemoryStream memoryStream = new MemoryStream();
                    response.GetResponseStream().CopyTo(memoryStream);

                    return memoryStream.ToArray();
                }
            }

            catch (Exception e)
            {
                Console.WriteLine($"Exception caught: {e}");
            }

            return null;
        }




        //免费 
        static  int freeCount = 0;
        public byte[] Free(string content)
        {
            Interlocked.Increment(ref freeCount);

            if(freeCount%10==0)
            Console.WriteLine("现在免费的使用次数："+ freeCount);

            if (!int.TryParse(this.voiceType, out int result))
                result = 0;


            if (result == 0)
            {
                var request = HttpWebRequest.Create($"https://dict.youdao.com/dictvoice?audio={content}&le=zh") as HttpWebRequest;
                request.Accept = "*/*";
                request.Headers.Add("accept-encoding", "identity;q=1, *;q=0");
                request.Headers.Add("accept-language", "zh-CN,zh;q=0.9");
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36 Edg/112.0.1722.48";

                using var response = request.GetResponse();
                using var responseStream = response.GetResponseStream();
                using MemoryStream memoryStream = new MemoryStream();
                responseStream.CopyTo(memoryStream);
                return ConvertMp3ToWavAsync(memoryStream.ToArray());  
            }
            else
            {
                result--;

                result = Math.Max(0, result);

                var by= HuoShan.Syn(content, result);

                if (by == null)
                    return null;

                switch (result)
                {
                    case 9:
                    case 8:
                    case 7:
                    case 6:
                    case 5:
                    case 1:
                    case 0:
                        return  ConvertMp3ToWavAsync(by);

                    case 4:
                    case 3:
                    case 2: 
                        return ConvertWavToWavAsync(by);
                }
                //mp3 10 9 8 7 6 2 1
                //wav 5 3 4
            }

            return null;
        }


        internal class HuoShan
        {
            readonly static string[] SpeakerID = new string[]
            {
           "zh_male_rap"  ,
            "zh_male_zhubo",
            "zh_female_zhubo",
            "tts.other.BV021_streaming",
            "tts.other.BV026_streaming",
            "tts.other.BV025_streaming",
            "zh_female_sichuan",
            "zh_male_xiaoming",
            "zh_female_qingxin",
            "zh_female_story",
            };

            public sealed class RequestJson
            {
                public string text { get; set; }
                public string speaker { get; set; }
                public string language { get; set; }
            }

            public static byte[] Syn(string content, int speakerID)
            {
                int count = 0;

                try
                {
                    HttpWebRequest httpWeb = WebRequest.Create("https://translate.volcengine.com/web/tts/v1/?msToken=&X-Bogus=DFSzswVLQDVxShxXtze4cBt/pLwh&_signature=_02B4Z6wo00001HGHldQAAIDB-swONFduFqhxh5FAAHkSBlWMUNlxgUAe4s1ikFNXJaiqFJ6mSq8rcIswpFuWECBzccf8IIICyL5JvBYytRJKTySllqZeXJEPwMr2367i2P4ohf6O7.4XGT1l0b") as HttpWebRequest;
                    httpWeb.Method = "POST";
                    httpWeb.Referer = "https://translate.volcengine.com/?category=&home_language=zh&source_language=detect&target_language=en&text=%E6%98%AF%E5%BE%B7%E5%9B%BD%E4%BA%BA%E7%9A%84%E5%90%8E%E5%8F%B0%E4%BA%BA%E5%91%98%E5%92%8C";
                    httpWeb.Accept = "application/json, text/plain, */*";
                    httpWeb.ContentType = "application/json";
                    httpWeb.Headers.Add("accept-encoding", "identity;q=1, *;q=0");
                    httpWeb.Headers.Add("accept-language", "zh-CN,zh;q=0.9");
                    httpWeb.Headers.Add("Origin", "https://translate.volcengine.com");
                    httpWeb.Headers.Add("Cookie", "i18next=zh-CN; s_v_web_id=verify_lo781l65_U3AWd0hy_ixyl_4bQo_Bgkc_HZlRRQYelo1W; ttcid=000f1b8ba7e9497a992e592941c7ec1611; hasUserBehavior=1; _tea_utm_cache_3569={%22utm_source%22:%225%22%2C%22utm_medium%22:%22sembaidu%22%2C%22utm_campaign%22:%22vgbdpzztnA003%22%2C%22utm_term%22:%22sem_baidu_vg_pinzhuan_01%22%2C%22utm_content%22:%22guanwang%22}; ve_doc_history=6561; digest=d80a51da-3e4c-4326-8ecb-c17c4a1a4530; AccountID=2100669638; referrer_title=%E5%9C%A8%E7%BA%BF%E8%AF%AD%E9%9F%B3%E5%90%88%E6%88%90%E8%80%81%E4%BA%BA; __tea_cache_tokens_3569={%22web_id%22:%227305064001178863139%22%2C%22user_unique_id%22:%227305064001178863139%22%2C%22timestamp%22:1700924957874%2C%22_type_%22:%22default%22}; x-jupiter-uuid=17015117792108166; tt_scid=OnrBP-WOFNVybuLvH-srI02vmkx20faMu1s1XvFKFMSmEKmmdhBcgFvq-vrFJryf1e5c");
                    httpWeb.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36 Edg/112.0.1722.48";
                    httpWeb.Host = "translate.volcengine.com";
                    using (Stream requestStream = httpWeb.GetRequestStream())
                    {
                        RequestJson requestJson = new RequestJson
                        {
                            text = content,
                            speaker = SpeakerID[speakerID >= SpeakerID.Length ? 0 : speakerID],
                            language = "zh"
                        };

                        var json = JsonConvert.SerializeObject(requestJson);

                        byte[] buffer = Encoding.UTF8.GetBytes(json);
                        requestStream.Write(buffer, 0, buffer.Length);
                    }
                    using WebResponse s = httpWeb.GetResponse();
                    using Stream stream = s.GetResponseStream();
                    int c;

                    byte[] p = new byte[1024 * 1024 * 5];
                    do
                    {
                        c = stream.Read(p, count, p.Length - count);
                        count += c;
                    }
                    while (c != 0);
                    string result = Encoding.UTF8.GetString(p, 0, count);
                    string pattern = "\"data\":\\s*\"([^\"]+)\"";
                    Match match = Regex.Match(result, pattern);

                    if (match.Success)
                    {
                        string data = match.Groups[1].Value;
                        return Convert.FromBase64String(data);
                    }
                    else
                    {
                        return null;

                    }
                }
                catch (Exception ex)
                {

                }
                return null;

            }


        }



        static byte[] ConvertMp3ToWavAsync(byte[] mp3Data, int sampleRate = 16000)
        {

            using var ffmpegProcess = new Process();
            ffmpegProcess.StartInfo.FileName = "ffmpeg";
            ffmpegProcess.StartInfo.Arguments = $"-hide_banner -loglevel error -f mp3 -i pipe:0 -f wav -ar {sampleRate} pipe:1";
            ffmpegProcess.StartInfo.UseShellExecute = false;
            ffmpegProcess.StartInfo.RedirectStandardInput = true;  // 启用标准输入
            ffmpegProcess.StartInfo.RedirectStandardOutput = true; // 启用标准输出
            ffmpegProcess.StartInfo.RedirectStandardError = true;
            ffmpegProcess.StartInfo.CreateNoWindow = true;

            // 启动 FFmpeg
            ffmpegProcess.Start();

            ffmpegProcess.StandardInput.BaseStream.Write(mp3Data, 0, mp3Data.Length);
            ffmpegProcess.StandardInput.Close(); // 关闭输入流

            // 异步读取 WAV 输出
            using var ms = new MemoryStream();
            ffmpegProcess.StandardOutput.BaseStream.CopyTo(ms);

            ffmpegProcess.WaitForExit();

            return ms.ToArray();
        }


        static byte[] ConvertWavToWavAsync(byte[] mp3Data, int sampleRate = 16000)
        {

            using var ffmpegProcess = new Process();
            ffmpegProcess.StartInfo.FileName = "ffmpeg";
            ffmpegProcess.StartInfo.Arguments = $"-hide_banner -loglevel error -f wav -i pipe:0 -f wav -ar {sampleRate} pipe:1";
            ffmpegProcess.StartInfo.UseShellExecute = false;
            ffmpegProcess.StartInfo.RedirectStandardInput = true;  // 启用标准输入
            ffmpegProcess.StartInfo.RedirectStandardOutput = true; // 启用标准输出
            ffmpegProcess.StartInfo.RedirectStandardError = true;
            ffmpegProcess.StartInfo.CreateNoWindow = true;

            // 启动 FFmpeg
            ffmpegProcess.Start();

            // 同步写入 MP3 数据到标准输入
            ffmpegProcess.StandardInput.BaseStream.Write(mp3Data, 0, mp3Data.Length);
            ffmpegProcess.StandardInput.Close(); // 关闭输入流

            // 异步读取 WAV 输出
            using var ms = new MemoryStream();
            ffmpegProcess.StandardOutput.BaseStream.CopyTo(ms);

            ffmpegProcess.WaitForExit();

            return ms.ToArray();
        }


    }
}
