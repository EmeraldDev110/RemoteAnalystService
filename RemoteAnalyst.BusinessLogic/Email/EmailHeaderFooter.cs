using System.Text;

namespace RemoteAnalyst.BusinessLogic.Email {
    public class EmailHeaderFooter {
        public string EmailHeader(bool isLocalAnalyst) {
            string return_str = " <body leftMargin=0 topMargin=0>";
            return_str += "<TABLE width=650 border=0 style='BORDER: #cccccc 1px solid;'>";
            //if(!isLocalAnalyst)
                return_str += "	<TR><TD style='HEIGHT: 35px'><IMG src='cid:ralogo_email.gif'></TD></TR>";
            return_str += "	<TR>";
            return_str += "		<TD bgColor='#f1f5fb'>";
            return_str += "			<blockquote>";
            return_str += "				<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>";

            return return_str;
        }

        public string EmailHeaderFullWidth(bool isLocalAnalyst) {
            string return_str = " <body leftMargin=0 topMargin=0>";
            return_str += "<TABLE width=100% border=0 style='BORDER: #cccccc 1px solid;'>";
            //if (!isLocalAnalyst)
                return_str += "	<TR><TD style='HEIGHT: 35px'><IMG src='cid:ralogo_email.gif'></TD></TR>";
            return_str += "	<TR>";
            return_str += "		<TD bgColor='#f1f5fb'>";
            return_str += "			<blockquote>";
            return_str += "				<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>";

            return return_str;
        }

        public string EmailHeaderFullWidth(string fName, string lName, bool isLocalAnalyst) {
            string return_str = " <body leftMargin=0 topMargin=0>";
            return_str += "<TABLE width=100% border=0 style='BORDER: #cccccc 1px solid;'>";
            //if (!isLocalAnalyst)
                return_str += "	<TR><TD style='HEIGHT: 35px'><IMG src='cid:ralogo_email.gif'></TD></TR>";
            return_str += "	<TR>";
            return_str += "		<TD bgColor='#f1f5fb'>";
            return_str += "			<blockquote>";
            return_str += "				<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>";
            return_str += "					<BR>Dear " + fName + " " + lName + ",<BR><BR>";

            return return_str;
        }

        public string EmailHeaderNewEmail(string fName, string lName, bool isLocalAnalyst) {
            string return_str = " <body style='font-family: Calibri;'>";
            
            //return_str += "<table width='1100px' border=0 style='border: #cccccc 1px solid;'>";
            return_str += "<table width='1100px' border=0>";
            //if (!isLocalAnalyst)
                return_str += "	<tr><td style='height: 18px;'><img src='cid:ralogo_email.gif'></TD></tr>";
            return_str += "	<tr>";
            return_str += "		<td>";
            return_str += "				<div style='font-family: Calibri; font-size: 9pt;'>";
            //return_str += "					<br>Dear " + fName + " " + lName + ",<br><br>";

            return return_str;
        }

        public string EmailHeaderDailyEmail(string message, bool isLocalAnalyst) {
            var returnStr = new StringBuilder();
            returnStr.Append(" <body style='font-family: Calibri;top:0px;'>");
            returnStr.Append("<table width='1100px' border=0>");
            returnStr.Append("	<tr>");
            //if(!isLocalAnalyst)
                returnStr.Append("<td style='height:18px;width:15%;'><img src='cid:ralogo_email.gif'></td>");
            returnStr.Append("<td align=left sytle='width:85%'>" + message + "</td>");
            returnStr.Append("</tr><tr><td colspan=2><div style='font-family: Calibri; font-size: 9pt;'>");

            return returnStr.ToString();
        }

        public string EmailHeader(string fName, string lName, bool isLocalAnalyst) {
            string return_str = " <body leftMargin=0 topMargin=0>";
            return_str += "<TABLE width=800 border=0 style='BORDER: #cccccc 1px solid;'>";
            //if (!isLocalAnalyst)
                return_str += "	<TR><TD style='HEIGHT: 35px'><IMG src='cid:ralogo_email.gif'></TD></TR>";
            return_str += "	<TR>";
            return_str += "		<TD bgColor='#f1f5fb'>";
            return_str += "			<blockquote>";
            return_str += "				<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>";
            return_str += "					<BR>Dear " + fName + " " + lName + ",<BR><BR>";

            return return_str;
        }

        public string EmailFooter(string supportEmail, string webSite) {
            string return_str = "     <BR><BR>";
            return_str += "			  </DIV>";
            return_str += "		   </blockquote>";
            return_str += "		   <hr size=1 width='95%'>";
            return_str += "		   <DIV style='FONT-SIZE: 7pt; COLOR: #666666; FONT-FAMILY: Arial' align='center'>";
            return_str += "			Please don't reply directly to this automatically generated email message.&nbsp; Should you ";
            return_str += "			have any questions, please contact us at <A href='" + supportEmail +
                          "'><FONT color='#0000ff'>";
            return_str += "			" + supportEmail + "</FONT></A><BR><br>";
            return_str += "		   </DIV>";
            return_str += "	     </TD>";
            return_str += "    </TR>";
            return_str += "  </TABLE>";
            return_str += "</body>";

            return return_str;
        }

        public string EmailFooterWithOutMessage(string supportEmail, string webSite, string mailTo) {
            string return_str = "   </DIV>";
            return_str += "		   <hr size=1 width='95%'>";
            return_str += "		   <DIV style='FONT-SIZE: 7pt; COLOR: #666666; FONT-FAMILY: Verdana' align='center'>";
            return_str += "			This is an automatic generated message. Please do not reply.&nbsp; Should you ";
            return_str += "			have any questions, please contact <A href='" + mailTo + "'><FONT color='#0000ff'>";
            return_str += "			" + supportEmail + "</FONT></A> or visit our website at <A href='" + webSite + "'>";
            return_str += "			<FONT color='#0000ff'>" + webSite + "</FONT></A> for more information.<BR><br>";
            return_str += "		   </DIV>";
            return_str += "	     </TD>";
            return_str += "    </TR>";
            return_str += "  </TABLE>";
            return_str += "</body>";

            return return_str;
        }

        public string EmailTunerHeader(bool isLocalAnalyst) {
            string return_str = " <body leftMargin=0 topMargin=0>";
            //if (!isLocalAnalyst)
                return_str += "	<IMG src='cid:ralogo_email.gif'></TD>";
            return_str += "	<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>";
            return return_str;
        }

        public string EmailHeaderSummary(bool isLocalAnalyst) {
            string return_str = " <body leftMargin=0 topMargin=0>";
            return_str += "<TABLE width=650 border=0 style='BORDER: #cccccc 1px solid;'>";
            //if (!isLocalAnalyst)
                return_str += "	<TR><TD style='HEIGHT: 35px'><IMG src='cid:ralogo_email.gif'></TD></TR>";
            return_str += "	<TR>";
            return_str += "		<TD bgColor='#f1f5fb'>";
            return_str += "				<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>";
            return_str += "<br /><br />";

            return return_str;
        }

        public string EmailFooterWithOutMessage(string supportEmail, string webSite) {
            string return_str = "   </DIV></blockquote>	";
            return_str += "		   <hr size=1 width='95%'>";
            return_str += "		   <DIV style='FONT-SIZE: 7pt; COLOR: #666666; FONT-FAMILY: Verdana' align='center'>";
            return_str += "			This is an automatic generated message. Please do not reply.&nbsp; Should you ";
            return_str += "			have any questions, please contact <A href='" + supportEmail +
                          "'><FONT color='#0000ff'>";
            return_str += "			" + supportEmail + "</FONT></A> or visit our website at <A href='" + webSite + "'>";
            return_str += "			<FONT color='#0000ff'>" + webSite + "</FONT></A> for more information.<BR><br>";
            return_str += "		   </DIV>";
            return_str += "	     </TD>";
            return_str += "    </TR>";
            return_str += "  </TABLE>";
            return_str += "</body>";

            return return_str;
        }
        public string EmailFooterWithOutBlockquote(string supportEmail, string webSite) {
            string return_str = "   </div>	";
            return_str += "		   <hr size=1 width='95%'>";
            return_str += "		   <div style='FONT-SIZE: 7pt; COLOR: #666666; FONT-FAMILY: Calibri' align='center'>";
            return_str += "			This is an automatic generated message. Please do not reply.&nbsp; Should you ";
            return_str += "			have any questions, please contact <a href='" + supportEmail +
                          "'><font color='#0000ff'>";
            return_str += "			" + supportEmail + "</font></a> or visit our website at <a href='" + webSite + "'>";
            return_str += "			<font color='#0000ff'>" + webSite + "</font></a> for more information.<br><br>";
            return_str += "		   </div>";
            return_str += "	     </td>";
            return_str += "    </tr>";
            return_str += "  </table>";
            return_str += "</body>";

            return return_str;
        }
    }
}