using System.Collections.Generic;
using System.Linq;

namespace TorrentHardLinkHelper.Locate
{
    public class LocateResult
    {
        private readonly IList<TorrentFileLink> _torrentFileLinks;
        private readonly LocateState _locateState;
        private readonly int _locatedCount;
        private readonly int _unlocatedCount;

        public LocateResult(IList<TorrentFileLink> torrentFileLinks)
        {
            this._torrentFileLinks = torrentFileLinks;
            this._locateState = this._torrentFileLinks.Any(c => c.State != LinkState.Located)
                ? LocateState.Fail
                : LocateState.Succeed;
            if (this._locateState == LocateState.Succeed)
            {
                this._locatedCount = this._torrentFileLinks.Count;
                this._unlocatedCount = 0;
            }
            else
            {
                this._locatedCount = this._torrentFileLinks.Count(c => c.State == LinkState.Located);
                this._unlocatedCount = this._torrentFileLinks.Count - this._locatedCount;
            }
        }

        public IList<TorrentFileLink> TorrentFileLinks
        {
            get { return this._torrentFileLinks; }
        }

        public LocateState LocateState
        {
            get { return this._locateState; }
        }

        public int LocatedCount
        {
            get { return this._locatedCount; }
        }

        public int UnlocatedCount
        {
            get { return this._unlocatedCount; }
        }
    }
}