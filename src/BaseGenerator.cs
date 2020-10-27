using System;

namespace FantasyMapGenerator
{
	public class BaseGenerator
	{
		private readonly GenerationContext _context;

		public BaseGenerator(GenerationContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			_context = context;
		}

		public void LogInfo(string message, params object[] args)
		{
			if (_context.InfoHandler == null)
			{
				return;
			}

			_context.InfoHandler(Utils.FormatMessage(message, args));
		}
	}
}
