﻿namespace NexiumCode.Services
{
    public interface IPasswordHash
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}
