using MvcModules;

namespace SimpleModule
{
    public class MvcModule : MvcModuleBase
    {
        public override string DefaultController { get { return "SimpleHome"; } }
        public override string DefaultAction { get { return "Index"; } }

        protected override void Init()
        {
            ScriptBundles.AddBundle("~/bundles/simplehome").Include("~/Scripts/simple-home.js");
        }
    }
}
