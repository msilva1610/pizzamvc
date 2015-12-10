﻿using KebabManager.Contracts.ViewModels;
using KebabManager.Web.Security;
using Pizza.Framework.Security;
using Pizza.Mvc.Controllers;

namespace KebabManager.Web.Controllers
{
    public class AccountController : AccountControllerBase<KebabUserViewModel, KebabPrincipalSerializeModel>
    {
        public AccountController(IAuthService membershipService) : base(membershipService)
        {
        }
    }
}