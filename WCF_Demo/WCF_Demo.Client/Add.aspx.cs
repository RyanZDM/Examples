using System;
using WCF_Demo.Dal;

namespace WCF_Demo.Client
{
	public partial class Add : System.Web.UI.Page
	{
		private readonly IAdd addClient = new AddClient("BasicHttpBinding_IAdd");

		protected void Page_Load(object sender, EventArgs e)
		{

		}

		protected void Button1_Click(object sender, EventArgs e)
		{
			var user = new User
			{
				UserID = 1,
				Name = this.TextBox1.Text,
				Password = this.TextBox2.Text,
				Describe = this.TextBox3.Text,
				SubmitTime = DateTime.Now
			};

			addClient.DoWork(user);
			Response.Write("Added successfully.");
		}
	}
}