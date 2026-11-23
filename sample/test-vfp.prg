* ChromiumActiveX VFP9 smoke test
* Run from the repository root with: DO sample\test-vfp.prg

LOCAL lcOldAutoYield, lcHtml, lcTitle, loErr
lcOldAutoYield = APPLICATION.AutoYield
APPLICATION.AutoYield = .F.

TRY

   oForm = CREATEOBJECT("ChromiumForm")
   oForm.Caption = "Chromium WebView2"
   oForm.Width  = 900
   oForm.Height = 650
   oForm.AutoCenter = .T.

   oForm.AddObject("web", "olecontrol", "ChromiumBrowser.WebView2")
   oForm.web.Left   = 0
   oForm.web.Top    = 0
   oForm.web.Width  = oForm.Width
   oForm.web.Height = oForm.Height - 50
   oForm.web.Visible = .T.

   oForm.AddObject("cmdClose", "CloseButton")
   oForm.cmdClose.Caption = "Close"
   oForm.cmdClose.Left   = oForm.Width - 110
   oForm.cmdClose.Top    = oForm.Height - 45
   oForm.cmdClose.Width  = 100
   oForm.cmdClose.Height = 30
   oForm.cmdClose.Visible = .T.

   oForm.Show()

   lcHtml = FULLPATH("sample\test.html")
   IF NOT FILE(lcHtml)
      lcHtml = FULLPATH("test.html")
   ENDIF
   STRTOFILE('<html><head><title>HelloChromium</title></head><body>OK</body></html>', lcHtml)
   oForm.web.Object.Navigate("file:///" + STRTRAN(lcHtml, "\", "/"))

   DO WHILE oForm.web.Object.ReadyState < 4
      DOEVENTS
   ENDDO

   ? "Title   :", oForm.web.Object.DocumentTitle
   ? "URL     :", oForm.web.Object.LocationURL
   ? "InitErr :", oForm.web.Object.InitializationError

   lcTitle = oForm.web.Object.ExecuteScript("document.title")
   lcTitle = STRTRAN(lcTitle, '"', '')
   ? "Script  :", lcTitle

   READ EVENTS

CATCH TO loErr
   ? "ERROR:", loErr.Message
FINALLY
   APPLICATION.AutoYield = lcOldAutoYield
ENDTRY

RETURN

DEFINE CLASS ChromiumForm AS Form
   PROCEDURE QueryUnload
      CLEAR EVENTS
   ENDPROC
ENDDEFINE

DEFINE CLASS CloseButton AS CommandButton
   PROCEDURE Click
      THISFORM.Release()
   ENDPROC
ENDDEFINE
