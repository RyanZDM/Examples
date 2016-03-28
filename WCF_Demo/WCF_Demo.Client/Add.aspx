<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Add.aspx.cs" Inherits="WCF_Demo.Client.Add" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
    </div>
		<asp:Label ID="Label1" runat="server" Text="ID:"></asp:Label>
    	<asp:TextBox ID="TextBox1" runat="server" TextMode="Number"></asp:TextBox>
		<br/>
		<asp:Label ID="Label2" runat="server" Text="Name:"></asp:Label>
    	<asp:TextBox ID="TextBox2" runat="server" TextMode="SingleLine"></asp:TextBox>
		<br/>
		<asp:Label ID="Label3" runat="server" Text="Password:"></asp:Label>
    	<asp:TextBox ID="TextBox3" runat="server" TextMode="Password"></asp:TextBox>
		<br/>
		<asp:Label ID="Label4" runat="server" Text="Describe:"></asp:Label>
    	<asp:TextBox ID="TextBox4" runat="server" TextMode="SingleLine"></asp:TextBox>
		<br/>
		<asp:Button ID="Button1" runat="server" Text="Button" OnClick="Button1_Click" />
    </form>
</body>
</html>
