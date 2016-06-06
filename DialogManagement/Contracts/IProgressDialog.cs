namespace DialogManagement.Contracts
{
	public interface IProgressDialog : IWaitDialog
	{
		int Progress { get; set; }
	}
}