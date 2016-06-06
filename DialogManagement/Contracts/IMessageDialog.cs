namespace DialogManagement.Contracts
{
	public interface IMessageDialog : IDialog
	{
		string Message { get; set; }
	}
}