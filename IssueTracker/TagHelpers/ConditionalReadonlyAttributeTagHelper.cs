using Microsoft.AspNetCore.Razor.TagHelpers;

namespace IssueTracker.Web.TagHelpers
{
    [HtmlTargetElement(Attributes = "attr-readonly")]
    public class ConditionalAttributeTagHelper : TagHelper
    {
        public bool AttrReadonly { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (AttrReadonly)
            {
                output.Attributes.Add(
                    context.TagName == "select" ? "disabled" : "readonly", "");
            }
        }
    }
}
