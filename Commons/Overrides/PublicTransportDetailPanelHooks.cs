using Klyte.Commons.Extensors;

namespace Klyte.Commons.LineList
{
    class PublicTransportDetailPanelHooks : Redirector<PublicTransportDetailPanelHooks>
    {                

        #region Hooking

        public override void AwakeBody()
        {
            doLog("Loading PublicTransportLineInfo Hooks!");
            AddRedirect(typeof(PublicTransportLineInfo).GetMethod("Awake", allFlags), semiPreventDefaultMI);
            
        }
        public override void doLog(string text, params object[] param)
        {
            return;
        }


        #endregion
    }
    

}
