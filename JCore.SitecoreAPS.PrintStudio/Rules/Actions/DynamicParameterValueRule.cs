using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Sitecore;
using Sitecore.Rules;
using Sitecore.Rules.Actions;

namespace JCore.SitecoreAPS.PrintStudio.Rules.Actions
{    
    public class DynamicParameterValueRule<T> : RuleAction<T> where T : RuleContext
    {
        public virtual string ParameterName { get; set; }

        /// <summary>
        /// Applies the specified rule context.
        /// </summary>
        /// <param name="ruleContext">The rule context.</param>
        public override void Apply(T ruleContext)
        {
            if (string.IsNullOrEmpty(this.ParameterName))
                return;
            if (!ruleContext.Parameters.ContainsKey(this.ParameterName))
                ruleContext.Parameters.Add(this.ParameterName, (object)this.ParameterValue());
            else
                ruleContext.Parameters[this.ParameterName] = (object)this.ParameterValue();
        }

        private string ParameterValue()
        {
            if (Context.Item != null)
            {
                return Context.Item.ID.ToString();
            }
            if (HttpContext.Current != null && !string.IsNullOrEmpty(HttpContext.Current.Request.QueryString.Get("id")))
            {
                return HttpContext.Current.Request.QueryString.Get("id");
            }
            return string.Empty;
        }    
    }
}
