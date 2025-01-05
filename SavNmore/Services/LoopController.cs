namespace savnmore.Services
{
    public class LoopController
    {
        public bool IsStopped { get; set; }
        public LoopController()
        {
            this.IsStopped = true;
        }
        public LoopController(bool startLoop)
        {
            this.IsStopped = !startLoop;
        }
        public void Start()
        {
            this.IsStopped = false;
        }
        public void Stop()
        {
            this.IsStopped = true;
        }
    }
}
