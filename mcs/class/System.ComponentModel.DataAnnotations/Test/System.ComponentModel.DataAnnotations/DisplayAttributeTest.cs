using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace MonoTests.System.ComponentModel.DataAnnotations
{
#if NET_4_0
	public class AttributeTargetValidation
	{
		[Display(ResourceType = typeof(GoodResources), Name = "NameKey")]
		public string WorksOnProperty { get; set; }

		[Display(ResourceType = typeof(GoodResources), Name = "NameKey")]
		public string WorksOnMethod ()
		{
			return "";
		}

		[Display(ResourceType = typeof(GoodResources), Name = "NameKey")]
		public string worksOnField;

		public string WorksOnParameter ([Display(ResourceType = typeof(GoodResources), Name = "NameKey")] string parameter1)
		{
			return "";
		}
	}
	
	public class GoodResources
	{
		public static string Name {
			get { return "NameValue"; }
		}
		public static string ShortName {
			get { return "ShortNameValue"; }
		}
		public static string Prompt {
			get { return "PromptValue"; }
		}
		public static string Description {
			get { return "DescriptionValue"; }
		}
		public static string GroupName {
			get { return "GroupNameValue"; }
		}
		
	}
	public class BadResources
	{
		private static string PrivateString {
			get { return "Not a public string"; }
		}
		public string InstanceString {
			get { return "Not a static string"; }
		}
		public string WriteOnlyString {
			set { }
		}
	}
	internal class InvisibleResources
	{
		public static string InvisibleResource {
			get { return "Not a visible string "; }
		}
	}
	
	[TestFixture]
	public class DisplayAttributeTests
	{
		const string property_not_set_message = "The {0} property has not been set.  Use the Get{0} method to get the value.";
		const string localization_failed_message = "Cannot retrieve property '{0}' because localization failed. Type '{1}' is not public or does not contain a public static string property with the name '{2}'.";
	
		[Test]
		public void StringProperties_ReturnLiteralValues_Success()
		{
			var display = new DisplayAttribute()
			{
				Name = "Name",
				ShortName = "ShortName",
				Prompt = "Prompt",
				Description = "Description",
				GroupName = "GroupName"
			};
			
			Assert.AreEqual("Name", display.GetName());
			Assert.AreEqual("ShortName", display.GetShortName());
			Assert.AreEqual("Prompt", display.GetPrompt());
			Assert.AreEqual("Description", display.GetDescription());
			Assert.AreEqual("GroupName", display.GetGroupName());
		}
		[Test]
		public void StringProperties_ReturnLocalizedValues_Success()
		{
			var display = new DisplayAttribute()
			{
				ResourceType = typeof(GoodResources),
				Name = "Name",
				ShortName = "ShortName",
				Prompt = "Prompt",
				Description = "Description",
				GroupName = "GroupName"
			};
			
			Assert.AreEqual(GoodResources.Name, display.GetName());
			Assert.AreEqual(GoodResources.ShortName, display.GetShortName());
			Assert.AreEqual(GoodResources.Prompt, display.GetPrompt());
			Assert.AreEqual(GoodResources.Description, display.GetDescription());
			Assert.AreEqual(GoodResources.GroupName, display.GetGroupName());
		}
		
		[Test]
		public void ShortName_ReturnsName_WhenNotSet()
		{
			var display = new DisplayAttribute()
			{
				Name = "Name"
			};
			
			Assert.AreEqual("Name", display.GetShortName());
		}
		
		[Test]
		public void OrderAndAutoGenerateProperties_Success()
		{
			var display = new DisplayAttribute()
			{
				Order = 1,
				AutoGenerateField = true,
				AutoGenerateFilter = false
			};
			
			Assert.AreEqual(1, display.Order);
			Assert.AreEqual(1, display.GetOrder());
			
			Assert.AreEqual(true, display.AutoGenerateField);
			Assert.AreEqual(true, display.GetAutoGenerateField());
			
			Assert.AreEqual(false, display.AutoGenerateFilter);
			Assert.AreEqual(false, display.GetAutoGenerateFilter());
		}
		
		[Test]
		public void StringProperties_GetUnSetProperties_ReturnsNull ()
		{
			var display = new DisplayAttribute ();
			Assert.IsNull (display.Name);
			Assert.IsNull (display.ShortName);
			Assert.IsNull (display.Prompt);
			Assert.IsNull (display.Description);
			Assert.IsNull (display.GroupName);
			
			Assert.IsNull (display.GetName ());
			Assert.IsNull (display.GetShortName ());
			Assert.IsNull (display.GetPrompt ());
			Assert.IsNull (display.GetDescription ());
			Assert.IsNull (display.GetGroupName ());
		}
		
		[Test]
		public void OrderAndAutoGeneratedProperties_GetUnSetProperties_ThrowsInvalidOperationException ()
		{
			var display = new DisplayAttribute();
			
			ExceptionAssert.Throws<InvalidOperationException>(() => display.Order.ToString(), string.Format(property_not_set_message, "Order"));
			ExceptionAssert.Throws<InvalidOperationException>(() => display.AutoGenerateField.ToString(), string.Format(property_not_set_message, "AutoGenerateField"));
			ExceptionAssert.Throws<InvalidOperationException>(() => display.AutoGenerateFilter.ToString(), string.Format(property_not_set_message, "AutoGenerateFilter"));
		}
		
		[Test]
		public void AllProperties_InvisibleResource_ThrowsInvalidOperationException ()
		{
			var resourceType = typeof(InvisibleResources);
			var resourceKey = "InvisibleResource";
			var display = new DisplayAttribute()
			{
				ResourceType = resourceType,
				Name = resourceKey,
				ShortName = resourceKey,
				Prompt = resourceKey,
				Description = resourceKey,
				GroupName = resourceKey
			};
			
			ExceptionAssert.Throws<InvalidOperationException>(() => display.GetName(), string.Format(localization_failed_message, "Name", resourceType, resourceKey));
			ExceptionAssert.Throws<InvalidOperationException>(() => display.GetShortName(), string.Format(localization_failed_message, "ShortName", resourceType, resourceKey));
			ExceptionAssert.Throws<InvalidOperationException>(() => display.GetPrompt(), string.Format(localization_failed_message, "Prompt", resourceType, resourceKey));
			ExceptionAssert.Throws<InvalidOperationException>(() => display.GetDescription(), string.Format(localization_failed_message, "Description", resourceType, resourceKey));
			ExceptionAssert.Throws<InvalidOperationException>(() => display.GetGroupName(), string.Format(localization_failed_message, "GroupName", resourceType, resourceKey));		
		}
		
		[Test]
		public void AllProperties_PrivateResource_ThrowsInvalidOperationException ()
		{
			var resourceType = typeof(BadResources);
			var resourceKey = "InstanceString";
			var display = new DisplayAttribute()
			{
				ResourceType = resourceType,
				Name = resourceKey,
				ShortName = resourceKey,
				Prompt = resourceKey,
				Description = resourceKey,
				GroupName = resourceKey
			};
			
			ExceptionAssert.Throws<InvalidOperationException>(() => display.GetName(), string.Format(localization_failed_message, "Name", resourceType, resourceKey));
			ExceptionAssert.Throws<InvalidOperationException>(() => display.GetShortName(), string.Format(localization_failed_message, "ShortName", resourceType, resourceKey));
			ExceptionAssert.Throws<InvalidOperationException>(() => display.GetPrompt(), string.Format(localization_failed_message, "Prompt", resourceType, resourceKey));
			ExceptionAssert.Throws<InvalidOperationException>(() => display.GetDescription(), string.Format(localization_failed_message, "Description", resourceType, resourceKey));
			ExceptionAssert.Throws<InvalidOperationException>(() => display.GetGroupName(), string.Format(localization_failed_message, "GroupName", resourceType, resourceKey));		
		}
		
		[Test]
		public void AllProperties_InstanceResource_ThrowsInvalidOperationException ()
		{
			var resourceType = typeof(BadResources);
			var resourceKey = "InstanceString";
			var display = new DisplayAttribute()
			{
				ResourceType = resourceType,
				Name = resourceKey,
				ShortName = resourceKey,
				Prompt = resourceKey,
				Description = resourceKey,
				GroupName = resourceKey
			};
			
			ExceptionAssert.Throws<InvalidOperationException>(() => display.GetName(), string.Format(localization_failed_message, "Name", resourceType, resourceKey));
			ExceptionAssert.Throws<InvalidOperationException>(() => display.GetShortName(), string.Format(localization_failed_message, "ShortName", resourceType, resourceKey));
			ExceptionAssert.Throws<InvalidOperationException>(() => display.GetPrompt(), string.Format(localization_failed_message, "Prompt", resourceType, resourceKey));
			ExceptionAssert.Throws<InvalidOperationException>(() => display.GetDescription(), string.Format(localization_failed_message, "Description", resourceType, resourceKey));
			ExceptionAssert.Throws<InvalidOperationException>(() => display.GetGroupName(), string.Format(localization_failed_message, "GroupName", resourceType, resourceKey));		
		}
		
		[Test]
		public void AllProperties_WriteOnlyResource_ThrowsInvalidOperationException ()
		{
			var resourceType = typeof(BadResources);
			var resourceKey = "WriteOnlyString";
			var display = new DisplayAttribute()
			{
				ResourceType = resourceType,
				Name = resourceKey,
				ShortName = resourceKey,
				Prompt = resourceKey,
				Description = resourceKey,
				GroupName = resourceKey
			};
			
			ExceptionAssert.Throws<InvalidOperationException>(() => display.GetName(), string.Format(localization_failed_message, "Name", resourceType, resourceKey));
			ExceptionAssert.Throws<InvalidOperationException>(() => display.GetShortName(), string.Format(localization_failed_message, "ShortName", resourceType, resourceKey));
			ExceptionAssert.Throws<InvalidOperationException>(() => display.GetPrompt(), string.Format(localization_failed_message, "Prompt", resourceType, resourceKey));
			ExceptionAssert.Throws<InvalidOperationException>(() => display.GetDescription(), string.Format(localization_failed_message, "Description", resourceType, resourceKey));
			ExceptionAssert.Throws<InvalidOperationException>(() => display.GetGroupName(), string.Format(localization_failed_message, "GroupName", resourceType, resourceKey));		
		}
	}
	public static class ExceptionAssert
	{
		public static void Throws<TException> (Action action, string expectedMessage) where TException : Exception
		{
			try
			{
				action ();
			}
			catch (TException ex)
			{
				Assert.AreEqual (expectedMessage, ex.Message);
				return;
			}
			
			Assert.Fail();
		}
	}
#endif
}
