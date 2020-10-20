using System;

namespace greenwallet.Persistence.Interfaces
{
    /// <summary>
    /// Optimistic concurrency conflict happened while repository was writing to data store
    /// </summary>
    public class ConcurrentWriteConflictException : Exception
    {
        
    }
}