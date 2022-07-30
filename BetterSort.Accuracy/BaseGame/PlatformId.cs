namespace BetterSort.Accuracy.External {
  using BetterSort.Common.External;
  using Newtonsoft.Json;
  using SiraUtil.Web;
  using Steamworks;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using IPALogger = IPA.Logging.Logger;

  public class PlayerId {
    internal PlayerId(IPALogger logger) {
      _logger = logger;
    }

    public string GetPlayerId() {

    }

    private readonly IPALogger _logger;
  }
}
