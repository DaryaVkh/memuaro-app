﻿using Memuaro.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Memuaro.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected readonly AuthProvider _authProvider;

    protected BaseController(AuthProvider authProvider)
    {
        _authProvider = authProvider;
    }
}