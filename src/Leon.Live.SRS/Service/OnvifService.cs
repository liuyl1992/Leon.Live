
using Mictlanix.DotNet.Onvif;
using Mictlanix.DotNet.Onvif.Common;
using System.Collections.Generic;

namespace Leon.Live.SRS
{
    public interface IOnvifService
    {
        Task<List<String>> DiscoverCamerasAsync(string ip, string name, string pwd);
    }

    public class OnvifService : IScopedDependency, IOnvifService
    {
        private readonly ILogger<OnvifService> _logger;
        public OnvifService(ILogger<OnvifService> logger)
        {
            _logger = logger;
        }

        public async Task<List<String>> DiscoverCamerasAsync(string ip, string username, string pwd)
        {
            var device = await OnvifClientFactory.CreateDeviceClientAsync(ip, username, pwd);
            var media = await OnvifClientFactory.CreateMediaClientAsync(ip, username, pwd);
            var ptz = await OnvifClientFactory.CreatePTZClientAsync(ip, username, pwd);
            var imaging = await OnvifClientFactory.CreateImagingClientAsync(ip, username, pwd);
            var caps = await device.GetCapabilitiesAsync(new CapabilityCategory[] { CapabilityCategory.All });
            bool absolute_move = false;
            bool relative_move = false;
            bool continuous_move = false;
            bool focus = false;

            Console.WriteLine("Capabilities");

            Console.WriteLine("\tDevice: " + caps.Capabilities.Device.XAddr);
            Console.WriteLine("\tEvents: " + caps.Capabilities.Events.XAddr);
            Console.WriteLine("\tImaging: " + caps.Capabilities.Imaging.XAddr);
            Console.WriteLine("\tMedia: " + caps.Capabilities.Media.XAddr);
            Console.WriteLine("\tPTZ: " + caps.Capabilities.PTZ.XAddr);

            var profiles = await media.GetProfilesAsync();
            string profile_token = null;

            Console.WriteLine("Profiles count :" + profiles.Profiles.Length);

            var streamSetup = new StreamSetup
            {
                Stream = StreamType.RTPUnicast,
                Transport = new Transport { Protocol = TransportProtocol.RTSP }
            };

            List<string> rtspPaths = new List<string>();
            foreach (var profile in profiles.Profiles)
            {
                Console.WriteLine($"Profile: {profile.token}");

                if (profile_token == null)
                {
                    profile_token = profile.token;
                    absolute_move = !string.IsNullOrWhiteSpace(profile.PTZConfiguration?.DefaultAbsolutePantTiltPositionSpace);
                    relative_move = !string.IsNullOrWhiteSpace(profile.PTZConfiguration?.DefaultRelativePanTiltTranslationSpace);
                    continuous_move = !string.IsNullOrWhiteSpace(profile.PTZConfiguration?.DefaultContinuousPanTiltVelocitySpace);
                }

                Console.WriteLine($"\tTranslation Support");
                Console.WriteLine($"\t\tAbsolute Translation: {!string.IsNullOrWhiteSpace(profile.PTZConfiguration?.DefaultAbsolutePantTiltPositionSpace)}");
                Console.WriteLine($"\t\tRelative Translation: {!string.IsNullOrWhiteSpace(profile.PTZConfiguration?.DefaultRelativePanTiltTranslationSpace)}");
                Console.WriteLine($"\t\tContinuous Translation: {!string.IsNullOrWhiteSpace(profile.PTZConfiguration?.DefaultContinuousPanTiltVelocitySpace)}");

                if (!string.IsNullOrWhiteSpace(profile.PTZConfiguration?.DefaultRelativePanTiltTranslationSpace))
                {
                    var pan = profile.PTZConfiguration?.PanTiltLimits.Range.XRange;
                    var tilt = profile.PTZConfiguration?.PanTiltLimits.Range.YRange;
                    var zoom = profile.PTZConfiguration?.ZoomLimits.Range.XRange;

                    Console.WriteLine($"\tPan Limits: [{pan?.Min}, {pan?.Max}] Tilt Limits: [{tilt?.Min}, {tilt.Max}] Tilt Limits: [{zoom?.Min}, {zoom?.Max}]");
                }
                var streamUri = await media.GetStreamUriAsync(streamSetup, profile.token);
                rtspPaths.Add(streamUri.Uri);
                _logger.LogInformation($"rtsp:{streamUri.Uri}");
            }
            return rtspPaths;
        }
    }
}
