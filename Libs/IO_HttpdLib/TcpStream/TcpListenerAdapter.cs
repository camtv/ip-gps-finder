using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace HttpdLib
{
    public class TcpListenerAdapter
    {
        protected readonly TcpListener _listener;

        public TcpListenerAdapter(TcpListener listener)
        {
            _listener = listener;
            _listener.Start();
        }

        public virtual async Task<TcpClientAdapter> GetClient()
        {
            return new TcpClientAdapter(await _listener.AcceptTcpClientAsync().ConfigureAwait(false));
        }

        public void Dispose()
        {
            _listener.Stop();
        }
    }

	public class SSlTcpListenerAdapter : TcpListenerAdapter
	{
		private readonly X509Certificate _certificate;

		public SSlTcpListenerAdapter(TcpListener listener, X509Certificate certificate) : base (listener)
		{
			_certificate = certificate;
		}

		public override async Task<TcpClientAdapter> GetClient()
		{
			return new SSlTcpClientAdapter(await _listener.AcceptTcpClientAsync().ConfigureAwait(false), _certificate);
		}
	}
}