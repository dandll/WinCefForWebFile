using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using System.IO;
using System.Text.RegularExpressions;

namespace WinCefSharpP2
{
    public partial class Form1 : Form
    {
        CefSharp.WinForms.ChromiumWebBrowser browser = null;

        public Form1()
        {
            InitializeComponent();
            CefSetting cs = new CefSetting();
            //必须进行初始化，否则就出来页面啦。
            CefSharp.Cef.Initialize(cs);

            //实例化控件
            browser = new CefSharp.WinForms.ChromiumWebBrowser("about:blank");
            //设置停靠方式
            browser.Dock = DockStyle.Fill;
            //加入到当前窗体中
            this.tabControl1.TabPages[0].Controls.Add(browser);
            //绑定新窗口打开事件
            //browser.LifeSpanHandler = new NewWindowCreatedEventHandler();

            browser.RequestHandler = new RequestHandler_new(browser.RequestHandler);//获取任意 资源的关键处。
            browser.FrameLoadStart += Browser_FrameLoadStart;
            browser.FrameLoadEnd += Browser_FrameLoadEnd;


            Control.CheckForIllegalCrossThreadCalls = false;//防止出现  线程间操作无效:
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            #region 防止多线程操作控件导致的卡死问题
            for (int i = 0; i < tabControl1.TabCount; i++)
            {
                tabControl1.SelectedIndex = 1;
            }
            tabControl1.SelectedIndex = 0;
            #endregion
        }

        /// <summary>
        /// 页面加载开始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browser_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
        {
            if (e.Url == browser.GetBrowser().GetFrame(browser.GetBrowser().GetFrameNames()[0]).Url)
            {
                lblTip.Text = "开始加载";
            }
        }

        /// <summary>
        /// 页面加载结束
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            try
            {
                if (e.Url == browser.GetBrowser().GetFrame(browser.GetBrowser().GetFrameNames()[0]).Url)
                {
                    lblTip.Text = "加载完成！";
                    if (cbSavaHtml.Checked)
                    {
                        //}
                        //if (e.Url.IndexOf("http://jandan.net/top") != -1)
                        //{
                        var task02 = e.Frame.GetSourceAsync();
                        task02.ContinueWith(t =>
                        {
                            if (!t.IsFaulted)
                            {
                                string resultStr = t.Result;
                                //Regex r = new Regex("(?<=<script(.)*?>)([\\s\\S](?!<script))*?(?=</script>)", RegexOptions.IgnoreCase);
                                {
                                    Regex r = new Regex("(<script(.)*?>)([\\s\\S](?!<script))*?(?=</script>)", RegexOptions.IgnoreCase);
                                    MatchCollection matches = r.Matches(resultStr);
                                    foreach (Match match in matches)
                                    {
                                        if (!string.IsNullOrEmpty(match.Value))
                                        {
                                            resultStr = resultStr.Replace(match.Value, "");
                                        }
                                    }
                                    resultStr = resultStr.Replace("</script>", "");
                                }
                                {
                                    Regex r = new Regex("(<link(.)*?>)([\\s\\S](?!>))*?", RegexOptions.IgnoreCase);
                                    MatchCollection matches = r.Matches(resultStr);
                                    foreach (Match match in matches)
                                    {
                                        if (!string.IsNullOrEmpty(match.Value) && match.Value.IndexOf(".js") != -1)
                                        {
                                            resultStr = resultStr.Replace(match.Value, "");
                                        }
                                    }
                                }
                                resultStr = resultStr.Replace("href=\"//", "href=\"http://");
                                WriteHtmlFile(resultStr, e.Url);
                                Log("获取成功！");
                            }
                        });
                        #region MyRegion
                        //StringBuilder sb = new StringBuilder();
                        //sb.AppendLine("function tempFunction() {");
                        ////sb.AppendLine(" return document.body.innerHTML; "); 
                        //sb.AppendLine(" return document.getElementsByTagName('html')[0].innerHTML; ");
                        //sb.AppendLine("}");
                        //sb.AppendLine("tempFunction();");
                        //var task01 = browser.GetBrowser().GetFrame(browser.GetBrowser().GetFrameNames()[0]).EvaluateScriptAsync(sb.ToString());
                        //task01.ContinueWith(t =>
                        //{
                        //    if (!t.IsFaulted)
                        //    {
                        //        var response = t.Result;
                        //        if (response.Success == true)
                        //        {
                        //            if (response.Result != null)
                        //            {
                        //                string resultStr = response.Result.ToString();
                        //                WriteHtmlFile(resultStr, e.Url);
                        //                #region MyRegion
                        //                //Regex r = new Regex("(/knowledge/package/[\\S]*.html)");//构造表达式package/scorm
                        //                //MatchCollection matches = r.Matches(resultStr);
                        //                //foreach (Match match in matches)
                        //                //{
                        //                //    string word = match.Groups["word"].Value;
                        //                //    int index = match.Index;
                        //                //    //richTextBox1.AppendText(match.Value.Replace("\"", ""));
                        //                //    //richTextBox1.AppendText("\r\n");
                        //                //    if (!PackageUrlList.Contains(match.Value.Replace("\"", "")))
                        //                //    {
                        //                //        PackageUrlList.Add(match.Value.Replace("\"", ""));
                        //                //    }
                        //                //}
                        //                //SetConfig(LastHistroyDataList, richTextBox1.Text);
                        //                //Log("获取成功！(添加" + matches.Count.ToString() + "列数据)"); 
                        //                #endregion
                        //                Log("获取成功！");
                        //            }
                        //        }
                        //    }
                        //}, TaskScheduler.FromCurrentSynchronizationContext());
                        #endregion
                        #region MyRegion
                        //var task = browser.GetBrowser().GetFrame(browser.GetBrowser().GetFrameNames()[0]).EvaluateScriptAsync(sb.ToString());
                        //task.ContinueWith(t =>
                        //{
                        //    if (!t.IsFaulted)
                        //    {
                        //        var response = t.Result;
                        //        if (response.Success == true)
                        //        {
                        //            if (response.Result != null)
                        //            {
                        //                string resultStr = response.Result.ToString();
                        //                WriteHtmlFile(resultStr, browser.GetBrowser().GetFrame(browser.GetBrowser().GetFrameNames()[0]).Url);
                        //                //Regex r = new Regex("(/knowledge/package/[\\S]*.html)");//构造表达式package/scorm
                        //                //MatchCollection matches = r.Matches(resultStr);
                        //                //foreach (Match match in matches)
                        //                //{
                        //                //    string word = match.Groups["word"].Value;
                        //                //    int index = match.Index;
                        //                //    //richTextBox1.AppendText(match.Value.Replace("\"", ""));
                        //                //    //richTextBox1.AppendText("\r\n");
                        //                //    if (!PackageUrlList.Contains(match.Value.Replace("\"", "")))
                        //                //    {
                        //                //        PackageUrlList.Add(match.Value.Replace("\"", ""));
                        //                //    }
                        //                //}
                        //                //SetConfig(LastHistroyDataList, richTextBox1.Text);
                        //                //Log("获取成功！(添加" + matches.Count.ToString() + "列数据)");
                        //                Log("获取成功！");
                        //            }
                        //        }
                        //    }
                        //}, TaskScheduler.FromCurrentSynchronizationContext());
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                Log("失败！(" + ex.Message + ")");
            }
        }

        /// <summary>
        /// 转到连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGo_Click(object sender, EventArgs e)
        {
            string url = textBox1.Text.Trim();
            //SetBrowserUrl(url);
            browser.Load(url);
        }
        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                if (browser != null)
                {
                    browser.Load(browser.Address);
                }
            }
        }
        /// <summary>
        /// 添加日志
        /// </summary>
        /// <param name="log"></param>
        void Log(string log)
        {
            rTxtLog.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "：" + log);
            rTxtLog.AppendText("\r\n");
            //写日志
            try
            {
                File.AppendAllText(("" + DateTime.Now.ToString("yyyyMMdd") + ".log"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "：" + log);
            }
            catch (Exception err)
            {
                File.AppendAllText(("" + DateTime.Now.ToString("yyyyMMdd") + ".log"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "：" + log);
            }
        }
        /// <summary>
        /// 写Html文件
        /// </summary>
        /// <param name="htmlStr"></param>
        void WriteHtmlFile(string htmlStr, string url)
        {
            rTxtLog.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "：" + "保存Html文件开始" + url);
            rTxtLog.AppendText("\r\n");
            try
            {
                //File.AppendAllText("html\\" + (GetLegalFileName(url) + "-" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".html"), htmlStr);

                string _directory = "DownloadFile/" + DateTime.Now.ToString("yyyyMMdd") + "/";
                File.AppendAllText(_directory + (GetLegalFileName(url) + "-" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".html"), htmlStr);
            }
            catch (Exception err)
            {
            }
        }
        /// <summary>
        /// 获取合法的文件名
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string GetLegalFileName(string str)
        {
            string unLegalStr = "/\\:*\"<>|？";
            for (int i = 0; i < unLegalStr.Length; i++)
            {
                string nowJudgeStr = unLegalStr.Substring(i, 1);
                while (str.IndexOf(nowJudgeStr) != -1)
                {
                    str = str.Replace(nowJudgeStr, "");
                }
            }
            return str;
        }

        #region 程序关闭处理
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
        }
        #endregion

        private void cbSavaHtml_CheckedChanged(object sender, EventArgs e)
        {
            (browser.RequestHandler as RequestHandler_new).savaFile = cbSavaHtml.Checked;
        }
    }
}
