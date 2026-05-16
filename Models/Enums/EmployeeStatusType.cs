using System;

namespace SecureTransparentDataExchange.Models.Enums
{
    /// <summary>
    /// Enum for employee status
    /// </summary>
    public enum EmployeeStatusType
    {
        Pending,        // Awaiting confirmation
        Active,         // Employee is active
        Terminated,     // Employee terminated
        ContractEnded   // Contract has ended
    }
}
