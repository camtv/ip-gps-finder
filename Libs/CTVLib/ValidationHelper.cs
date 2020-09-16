using System;
using System.Linq;

namespace Helpers
{
	public class ValidationHelper
	{
		public static bool isValidName(string name)
		{
			try
			{
				return name.All(char.IsLetter);
			}
			catch (Exception ex)
			{
				LogHelper.Error("Exception - {0}", ex.Message);
			}
			return false;
		}

		public static bool isValidGender(string gender)
		{
			try
			{
				return gender == "male" || gender == "female";
			}
			catch (Exception ex)
			{
				LogHelper.Error("Exception - {0}", ex.Message);
			}
			return false;
		}

		public static bool IsValidEmail(string email)
		{
			try
			{
				var addr = new System.Net.Mail.MailAddress(email);
				return addr.Address == email;
			}
			catch (Exception ex)
			{
				LogHelper.Error("Exception - {0}", ex.Message);
			}
			return false;
		}

		public static bool IsValidIBAN(string iban)
		{
			try
			{

			}
			catch (Exception ex)
			{
				LogHelper.Error("Exception - {0}", ex.Message);
			}
			return false;
		}


		public static bool CheckNoHtml(string text)
		{
			try
			{
				if (string.IsNullOrEmpty(text)) return true;

				var doc = new HtmlAgilityPack.HtmlDocument();
				doc.LoadHtml(text);

				return text == doc.DocumentNode.InnerText;
			}
			catch (Exception ex)
			{
				LogHelper.Error("Exception - {0}", ex.Message);
			}
			return false;
		}
	}
}
