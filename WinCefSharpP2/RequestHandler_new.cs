using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using System.IO;

namespace WinCefSharpP2
{
    public class RequestHandler_new : CefSharp.Handler.DefaultRequestHandler //CefSharp.Example.Handlers
    {
        public string _directory = "DownloadFile/";
        public bool savaFile = false;

        private Dictionary<UInt64, MemoryStreamResponseFilter> responseDictionary = new Dictionary<UInt64, MemoryStreamResponseFilter>();
        
        public IRequestHandler _requestHeandler;
        /// <summary>
        /// 实现cefsharp.irequestHandler并分配以处理与浏览器相关的事件请求
        /// </summary>
        /// <param name="rh"></param>
        public RequestHandler_new(IRequestHandler rh) : base()
        {
            _requestHeandler = rh;
        }

        /// <summary>
        /// 在加载资源请求之前调用。对于异步处理，返回cefsharp.cefseturnvalue.continueasync并执行cefsharp.irequestcallback.continue（system.boolean）或cefsharp.irequestcallback.cancel
        /// </summary>
        /// <param name="browserControl"></param>
        /// <param name="browser"></param>
        /// <param name="frame"></param>
        /// <param name="request"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public override CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            Uri url;
            if (Uri.TryCreate(request.Url, UriKind.Absolute, out url) == false)
            {
                //If we're unable to parse the Uri then cancel the request
                // avoid throwing any exceptions here as we're being called by unmanaged code
                return CefReturnValue.Cancel;
            }

            //System.Diagnostics.Debug.WriteLine(request.ResourceType.ToString());
            //System.Diagnostics.Debug.WriteLine(url);

            var extension = url.ToString().ToLower();
            if (CheckUpUrl(extension))
            {
                System.Diagnostics.Debug.WriteLine(url);//打印
            }

            #region 官方 示例
            //下面是一大波官方 示例
            //Uri url;
            //if (Uri.TryCreate(request.Url, UriKind.Absolute, out url) == false)
            //{
            //    //If we're unable to parse the Uri then cancel the request
            //    // avoid throwing any exceptions here as we're being called by unmanaged code
            //    return CefReturnValue.Cancel;
            //}

            ////Example of how to set Referer
            //// Same should work when setting any header

            //// For this example only set Referer when using our custom scheme
            //if (url.Scheme == CefSharpSchemeHandlerFactory.SchemeName)
            //{
            //    //Referrer is now set using it's own method (was previously set in headers before)
            //    request.SetReferrer("http://google.com", ReferrerPolicy.Default);
            //}

            ////Example of setting User-Agent in every request.
            ////var headers = request.Headers;

            ////var userAgent = headers["User-Agent"];
            ////headers["User-Agent"] = userAgent + " CefSharp";

            ////request.Headers = headers;

            ////NOTE: If you do not wish to implement this method returning false is the default behaviour
            //// We also suggest you explicitly Dispose of the callback as it wraps an unmanaged resource.
            ////callback.Dispose();
            ////return false;

            ////NOTE: When executing the callback in an async fashion need to check to see if it's disposed
            //if (!callback.IsDisposed)
            //{
            //    using (callback)
            //    {
            //        if (request.Method == "POST")
            //        {
            //            using (var postData = request.PostData)
            //            {
            //                if (postData != null)
            //                {
            //                    var elements = postData.Elements;

            //                    var charSet = request.GetCharSet();

            //                    foreach (var element in elements)
            //                    {
            //                        if (element.Type == PostDataElementType.Bytes)
            //                        {
            //                            var body = element.GetBody(charSet);
            //                        }
            //                    }
            //                }
            //            }
            //        }

            //        //Note to Redirect simply set the request Url
            //        //if (request.Url.StartsWith("https://www.google.com", StringComparison.OrdinalIgnoreCase))
            //        //{
            //        //    request.Url = "https://github.com/";
            //        //}

            //        //Callback in async fashion
            //        //callback.Continue(true);
            //        //return CefReturnValue.ContinueAsync;
            //    }
            //}

            //return CefReturnValue.Continue; 
            #endregion

            if (_requestHeandler != null)
            {
                return _requestHeandler.OnBeforeResourceLoad(browserControl, browser, frame, request, callback);
                //return base.OnBeforeResourceLoad(browserControl, browser, frame, request, callback);
            }

            return CefReturnValue.Continue;
        }

        /// <summary>
        /// 在CEF IO线程上调用以选择性地筛选资源响应内容。
        /// </summary>
        /// <param name="browserControl"></param>
        /// <param name="browser"></param>
        /// <param name="frame"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public override IResponseFilter GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            var url = new Uri(request.Url);
            var extension = url.ToString().ToLower();
            if (CheckUpUrl(extension))
            {
                //Only called for our customScheme
                var dataFilter = new MemoryStreamResponseFilter();//新建成数据 处理器
                responseDictionary.Add(request.Identifier, dataFilter);
                return dataFilter;
            }

            if (_requestHeandler != null)
            {
                return _requestHeandler.GetResourceResponseFilter(browserControl, browser, frame, request, response);
                //return base.GetResourceResponseFilter(browserControl, browser, frame, request, response);
            }
            return null;
        }

        Random _rand = new Random();
        /// <summary>
        /// 资源加载完成后在CEF IO线程上调用。
        /// </summary>
        /// <param name="browserControl"></param>
        /// <param name="browser"></param>
        /// <param name="frame"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="status"></param>
        /// <param name="receivedContentLength"></param>
        public override void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            var url = new Uri(request.Url);
            var extension = url.ToString().ToLower();
            //if (request.ResourceType == ResourceType.Image || extension.EndsWith(".jpg") || extension.EndsWith(".png") || extension.EndsWith(".gif") || extension.EndsWith(".jpeg"))

            #region 保存文件
            if (savaFile)
            {
                if (CheckUpUrl(extension))
                {
                    MemoryStreamResponseFilter filter;
                    if (responseDictionary.TryGetValue(request.Identifier, out filter))
                    {
                        #region 目录判断
                        _directory = "DownloadFile/" + DateTime.Now.ToString("yyyyMMdd") + "/";
                        if (!Directory.Exists(_directory))
                        {
                            Directory.CreateDirectory(_directory);
                        }
                        if (!Directory.Exists(_directory + "css/"))
                        {
                            Directory.CreateDirectory(_directory + "css/");
                        }
                        if (!Directory.Exists(_directory + "image/"))
                        {
                            Directory.CreateDirectory(_directory + "image/");
                        }
                        #endregion

                        System.Diagnostics.Debug.WriteLine("responseDictionary.Count:" + responseDictionary.Count);

                        //TODO: Do something with the data here
                        var data = filter.Data;
                        var dataLength = filter.Data.Length;
                        //NOTE: You may need to use a different encoding depending on the request
                        //var dataAsUtf8String = Encoding.UTF8.GetString(data);

                        if (dataLength > 0)
                        {
                            string fileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff-") + _rand.Next(99999, 999999) + ".png";
                            string path = _directory + fileName;

                            try
                            {
                                fileName = Path.GetFileName(url.ToString());
                                if (extension.EndsWith(".css"))
                                {
                                    path = _directory + "css/" + fileName;
                                }
                                else
                                {
                                    path = _directory + "image/" + fileName;
                                }

                                File.WriteAllBytes(path, data);
                                return;
                            }
                            catch (Exception e)
                            {
                                //throw;
                            }

                            //fileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff-") + _rand.Next(99999, 999999) + ".png";
                            //if (extension.EndsWith(".css"))
                            //{
                            //    path = _directory + "css/" + fileName;
                            //}
                            //else
                            //{
                            //    path = _directory + fileName;
                            //}
                            //if (!File.Exists(path))
                            //{
                            //    File.WriteAllBytes(path, data);//保存数据
                            //}
                        }
                    }
                    return;
                }
            }
            #endregion

            if (_requestHeandler != null)
            {
                _requestHeandler.OnResourceLoadComplete(browserControl, browser, frame, request, response, status, receivedContentLength);
            }
        }

        /// <summary>
        /// 检查Url是否符合规范（用于保存）
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        bool CheckUpUrl(string url)
        {
            if ((url.IndexOf("sinaimg.cn") > -1) && (url.EndsWith(".jpg") || url.EndsWith(".png") || url.EndsWith(".gif") || url.EndsWith(".jpeg"))) 
            {
                return true;
            }
            if (url.EndsWith(".css"))
            {
                return true;
            }
            return false;
        }
    }
}
