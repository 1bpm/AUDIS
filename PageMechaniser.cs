/*
 * Created by SharpDevelop.
 * User: rknight
 * Date: 19/03/2015
 * Time: 17:22
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Net;
using System.IO;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.IE;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support;
using System.Threading;


namespace AUDIS {
	/// <summary>
	/// the actual heavy lifting
	/// page automation with selenium
	/// </summary>
	public class PageMechaniser {
		//private PhantomJSDriver browser;
		//private InternetExplorerDriver browser;
		private FirefoxDriver browser=null;
		private int defaultTimeout=10;
		private int thisTimeout=10;
		private bool acceptNextAlert=true;
		private ScheduleInformation unifySchedule;
		private string baseWindowHandle;
		private string discoWindowHandle;
		private int popupNumber=0;
		
		/// <summary>
		/// the session construct
		/// </summary>
		/// <param name="scheduled"></param>
		public PageMechaniser(ScheduleInformation scheduled) {
			unifySchedule=scheduled;
			
			//InternetExplorerOptions options=new InternetExplorerOptions();
			//options.IgnoreZoomLevel=true;
			//options.EnsureCleanSession=true;
			//browser=new InternetExplorerDriver(options);

    
			
			
			
			
			
			
			
			
			//FirefoxBinary fb=new FirefoxBinary(Settings.firefoxPath);
			
			FirefoxProfile fp=new FirefoxProfile(@"M:\knightr-store\ffProfile");
			fp.WriteToDisk();
			browser=new FirefoxDriver(new FirefoxBinary(),fp,TimeSpan.FromSeconds(unifySchedule.extract.maxTimeout));
			
			
			//PhantomJSOptions opt=new PhantomJSOptions();
			
			//browser=new PhantomJSDriver(@"C:\devWork\SELtst\",opt,TimeSpan.FromSeconds(unifySchedule.extract.maxTimeout));
			
			
			Thread.Sleep(500);
			browser.Manage().Cookies.DeleteAllCookies();
			if (!Settings.debug) {
				browser.Manage().Window.Position=new System.Drawing.Point(-2000,0);
			}
			Thread.Sleep(1000);
			RunSequence();
			
		}
		
		~PageMechaniser() {
			if (browser!=null) {
				browser.Close();
				browser.Quit();
			}
		}
		
		
		private void RunSequence() {
			DefaultLogin();
			DiscoViewer();
			SelectAndFocusIn();
			
			string theExtractData;
			
			theExtractData=ExportAndProcess();
			
			browser.Close();
			browser.Quit();
			unifySchedule.ImportData(theExtractData);
		}
		
		private void DefaultLogin() {
			browser.Navigate().GoToUrl(Settings.baseURL);
			if (IsElementPresent(By.LinkText("Sign In"),20)) {
				browser.FindElement(By.LinkText("Sign In")).Click();
				
				browser.FindElement(By.Id("txtUsername")).Clear();
				browser.FindElement(By.Id("txtUsername")).SendKeys(Settings.unifyUser);
				
				browser.FindElement(By.Id("txtPassword")).Clear();
				browser.FindElement(By.Id("txtPassword")).SendKeys(Settings.unifyPass);
				
				Thread.Sleep(500);
				browser.FindElement(By.Id("bttLogin")).Click();
			}
		}
		
		public void DiscoViewer() {
			baseWindowHandle=browser.CurrentWindowHandle;
			browser.FindElement(By.Id("Banner_lnkDiscoViewer")).Click();
			
			
			foreach (string winHandle in browser.WindowHandles) {
				browser.SwitchTo().Window(winHandle);
				if (browser.Title=="Worksheet List") {
					discoWindowHandle=winHandle;
					goto outOfLoop;
				}
			}
		outOfLoop:
			Program.Log("Got disco viewer");
		}
		
		public void SetPageTimeout(int seconds) {
			thisTimeout=seconds;	
		}
		
		private void SetTimeout(int howLong) {
			browser.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(Convert.ToDouble(howLong)));
		}
		
		private void SetTimeout() {
			SetTimeout(defaultTimeout);
		}
		
		private bool IsElementPresent(By reference,int timeoutLen) {
			WebDriverWait wait=new WebDriverWait(browser,TimeSpan.FromSeconds(timeoutLen));
			wait.Until( ExpectedConditions.ElementExists(reference));
			return true;
		}
		
		private bool IsxElementPresent(By htmlReference,int howLongToWait) {
			SetTimeout(howLongToWait);

			try {
				browser.FindElement(htmlReference);
				return true;
			} catch (NoSuchElementException e) {
				return false;
			} finally {
				SetTimeout();
			}
		}

		
		
		
		private string HTEncode(string input) {
			return Uri.EscapeDataString(input).
				Replace("%20","+").
				Replace("(","%28").
				Replace(")","%29");
			
		}
		
		private void SelectAndFocusIn() {
			this.IsElementPresent(By.XPath("//*[@id='"+
			                             HTEncode(unifySchedule.extract.bookName)+
			                             "']/../../../td/a[contains(@href,\"javascript:expl_hg.focusNode\")]"),120);
			browser.FindElement(By.XPath("//*[@id='"+
			                             HTEncode(unifySchedule.extract.bookName)+
			                             "']/../../../td/a[contains(@href,\"javascript:expl_hg.focusNode\")]")).Click();
			this.IsElementPresent(By.Id(HTEncode(unifySchedule.extract.extractName)),120);
			browser.FindElement(By.Id(HTEncode(unifySchedule.extract.extractName))).Click();
			
			
			
			// if monthly / month based extract
			Program.Log("going for period "+unifySchedule.period);
			
			if (null!=unifySchedule.extract.extractAdditionalHt) {
				Program.Log("extractadditional ht is not null and period is "+unifySchedule.period);
				if (unifySchedule.extract.extractAdditionalHt.Contains("WEday")
				    && unifySchedule.extract.extractAdditionalHt.Contains("year")) {
				    	string[] delim = new string[1];
						delim[0]=";";
						string[] items=unifySchedule.extract.extractAdditionalHt.Split(delim,StringSplitOptions.None);
						foreach(string tItem in items) {
							
							if (tItem.StartsWith("year")) {
								string yearField=tItem.Replace("year=","");
								this.IsElementPresent(By.Id(yearField),60);
								new SelectElement(browser.FindElement(By.Id(yearField))).SelectByText(unifySchedule.strYear);	
							}
							if (tItem.StartsWith("WEday")) {
								string dayField=tItem.Replace("WEday=","");
								this.IsElementPresent(By.Id(dayField),60);
								new SelectElement(browser.FindElement(By.Id(dayField))).SelectByText(unifySchedule.period);
							}
						}
					
					
					
				} else if (unifySchedule.extract.extractAdditionalHt.Contains("month")
				    && unifySchedule.extract.extractAdditionalHt.Contains("year")) {
					string[] delim = new string[1];
						delim[0]=";";
						string[] items=unifySchedule.extract.extractAdditionalHt.Split(delim,StringSplitOptions.None);
						foreach(string tItem in items) {
							
							if (tItem.StartsWith("year")) {
								string yearField=tItem.Replace("year=","");
								this.IsElementPresent(By.Id(yearField),60);
								new SelectElement(browser.FindElement(By.Id(yearField))).SelectByText(unifySchedule.strYear);	
							}
							if (tItem.StartsWith("month")) {
								string monthField=tItem.Replace("month=","");
								this.IsElementPresent(By.Id(monthField),60);
								new SelectElement(browser.FindElement(By.Id(monthField))).SelectByText(unifySchedule.strMonthTxt);
							}
						}
					
					
				} else if (unifySchedule.extract.extractAdditionalHt.StartsWith("WEday=")==true) {
					
					string weDayField=unifySchedule.extract.extractAdditionalHt.Replace("WEday=","");
					this.IsElementPresent(By.Id(weDayField),60);
					browser.FindElement(By.Id(weDayField)).Clear();
					browser.FindElement(By.Id(weDayField)).SendKeys(unifySchedule.period);
				} else if (unifySchedule.extract.extractAdditionalHt.StartsWith("day=")==true) {
					//string 

					
					// daily freaking sitreps
					
				} else if (unifySchedule.extract.extractAdditionalHt.StartsWith("quarterStart=")==true) {
					Program.Log("ok, quarter start");
					string singleDayField=unifySchedule.extract.
						extractAdditionalHt.Replace("quarterStart=","");
					this.IsElementPresent(By.Id(singleDayField),60);
					browser.FindElement(By.Id(singleDayField)).Clear();
					browser.FindElement(By.Id(singleDayField)).SendKeys(unifySchedule.singleDayDate);
					Program.Log("set day as "+unifySchedule.singleDayDate);
				} 
			} else { // regular month / year vibe
				this.IsElementPresent(By.Id(unifySchedule.extract.extractYearHtID),60);
				browser.FindElement(By.Id(unifySchedule.extract.extractYearHtID)).Clear();
				browser.FindElement(By.Id(unifySchedule.extract.extractYearHtID)).SendKeys(unifySchedule.strYear);
				browser.FindElement(By.Id(unifySchedule.extract.extractPeriodHtID)).Clear();
				browser.FindElement(By.Id(unifySchedule.extract.extractPeriodHtID)).SendKeys(unifySchedule.strMonthTxt);
			
			}
			
			
			SetTimeout(unifySchedule.extract.maxTimeout);
			
			IsElementPresent(By.CssSelector("img[alt=\"Go\"]"),60);
			browser.FindElement(By.CssSelector("img[alt=\"Go\"]")).Click();
	

			//if (!IsElementPresent(By.Id("export"),unifySchedule.extract.maxTimeout)) {
			//	Program.Log("Unify did not return overview data within the given timeout");
				//fatalz
			//}
					
			// wait until clickable by id 'export'
			
			if (unifySchedule.extract.setPopups!=null) {
				Program.Log("checking/setting popups");
				string originalWindow=browser.CurrentWindowHandle;
				foreach (string popup in unifySchedule.extract.setPopups) {
			//if (!IsElementPresent(By.Id("export"),unifySchedule.extract.maxTimeout)) {
			//	Program.Log("Unify did not return overview data within the given timeout");
				//fatalz
			//}
					
					
					browser.SwitchTo().Window(originalWindow);
					Program.Log("attempting to find "+popup);
					if (IsElementPresent(By.Id(popup.Trim()),unifySchedule.extract.maxTimeout)) { // hack on time
						PopupConditionsSetter(popup.Trim());
						browser.SwitchTo().Window(originalWindow);
					}
					
					
					

					
					
					
				}
				// return to DISCO viewer
				browser.SwitchTo().Window(originalWindow);
			} // end of set popups
			
			
			
			Thread.Sleep(500);
			

			
			
			
			
						if (!IsElementPresent(By.Id("export"),unifySchedule.extract.maxTimeout)) {
				Program.Log("Unify did not return overview data within the given timeout");
				//fatalz
			}
			
			Program.Log("ready to export...");
			
		}
		
		private IWebElement SelectPopup(By anElementThatIsExpected) {
			bool checkForWindow = true;
			IWebElement innerFrame=null;
			
			while (checkForWindow) {
			
			foreach (string handle in browser.WindowHandles) {
					if (handle!=discoWindowHandle &&  handle!=baseWindowHandle) {
					
				Program.Log("checking da "+handle);
				browser.SwitchTo().Window(handle);
				browser.SwitchTo().Frame(0);
				innerFrame=browser.SwitchTo().ActiveElement();
				bool conditionsPresent=IsElementPresentIn(anElementThatIsExpected,innerFrame,5);
				
				if (conditionsPresent) {
					checkForWindow=false;
					return innerFrame;					
				}
			}
				}
				
				
			}
			throw new Exception("cannot get popup!");
			return null;
		}
		
		
		private void PopupConditionsSetter(string theID) {
			string internalID="_bi_strlc$lov"+popupNumber.ToString();
			popupNumber++;
			IWebElement innerFrame=null;
			 new SelectElement(browser.FindElement(By.Id(theID))).SelectByText("More...");
			
			bool checkForWindow = true;
	
			while (checkForWindow) {
			
			foreach (string handle in browser.WindowHandles) {
					if (handle!=discoWindowHandle &&  handle!=baseWindowHandle) {
					
				Program.Log("checking da "+handle);
				Thread.Sleep(500);
				browser.SwitchTo().Window(handle);
				browser.SwitchTo().Frame(0);
				innerFrame=browser.SwitchTo().ActiveElement();
				bool conditionsPresent=IsElementPresentIn(By.Id(internalID),innerFrame,10);
				
				if (conditionsPresent) {
					checkForWindow=false;				
					goto PopupFound;
				}
			}
				}
				
				
			}
//			Program.Log("Popup expected: no popup found.");
//			return;
	
	
			//sleep
			
			
		PopupFound:		
			Program.Log("Popup found");
			if (IsElementPresentIn(By.Id(internalID),innerFrame,20)) {
				innerFrame.FindElement(By.Id(internalID)).Clear();
				innerFrame.FindElement(By.Id(internalID)).SendKeys("<all>");
				IWebElement inImage=innerFrame.FindElement(By.CssSelector("img[alt=\"Go find members\"]"));
				inImage.FindElement(By.XPath("..")).Click();
			
	
			
			
				innerFrame=SelectPopup(By.Name("lc0:selected"));
				innerFrame.FindElement(By.Name("lc0:selected")).Click();
				
				
				// VHAT
				if (IsElementPresentIn(By.XPath("(//img[@alt='Select member: Access key = c'])[2]"),innerFrame,30)) {
					innerFrame.FindElement(By.XPath("(//img[@alt='Select member: Access key = c'])[2]")).Click();
				} else {
					Program.Log("could not submit popup");
				}
			} else {
				Program.Log("could not locate popup selector");
			}
		}
		
		
		private string ExportAndProcess() {
			
			if (!IsElementPresent(By.Id("export"),unifySchedule.extract.maxTimeout)) {
				throw new Exception("export not available within allocated time");
			}
			SetTimeout(unifySchedule.extract.maxTimeout);
			browser.FindElement(By.Id("export")).Click();
			
			SetTimeout(unifySchedule.extract.maxTimeout);
			if (!IsElementPresent(By.CssSelector("img[alt=\"Export\"]"),unifySchedule.extract.maxTimeout)) {
				throw new Exception("export not available within allocated time");
			}
			
			browser.FindElement(By.CssSelector("img[alt=\"Export\"]")).Click();
			
			
			
			SetTimeout(unifySchedule.extract.maxTimeout);
			if (!IsElementPresent(By.CssSelector("img[alt=\"Click to view or save\"]"),unifySchedule.extract.maxTimeout)) {
				Program.Log("Unify did not return data within the given timeout");
				//fatalz
			}
			
			// OK! export ready to 
			
			
			browser.FindElement(By.CssSelector("img[alt=\"Click to view or save\"]")).Click();
			foreach (string winHandle in browser.WindowHandles) {
				browser.SwitchTo().Window(winHandle);
			}
			string theSourceData=null;
			if (IsElementPresent(By.TagName("pre"),20)) {
				theSourceData=browser.FindElement(By.TagName("pre")).Text;
			} else {
				theSourceData=browser.PageSource;
			}
			return theSourceData;
		}
		
		private bool IsElementPresentIn(By htmlReference, IWebElement element, int returnTimeout) {
			SetTimeout(returnTimeout);
			
			try {
				element.FindElement(htmlReference);
				return true;
			} catch (NoSuchElementException e) {
				return false;
			} finally {
				SetTimeout(unifySchedule.extract.maxTimeout);
			}
		}
		
		private bool IsAlertPresent() {
			try {
				browser.SwitchTo().Alert();
				return true;
			} catch (NoAlertPresentException e) {
				return false;
			}
		}
		
		private string CloseAlertAndGetItsText() {
			string returnVal=null;
			try {
				IAlert alert= browser.SwitchTo().Alert();
				if (acceptNextAlert) {
					returnVal=alert.Text;
					alert.Accept();
				} else {
					alert.Dismiss();
				}
				return returnVal;
			} finally {
				acceptNextAlert=true;
				
			}
			
		}
	}
}
