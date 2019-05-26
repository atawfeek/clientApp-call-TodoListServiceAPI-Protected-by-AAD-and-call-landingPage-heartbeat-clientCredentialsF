using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TodoListServiceAPI_Protected_by_AAD.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class TodoListController : ControllerBase
    {
        //
        // To Do items list for all users.  Since the list is stored in memory, it will go away if the service is cycled.
        //
        static ConcurrentBag<TodoItem> todoBag = new ConcurrentBag<TodoItem>();

        // GET api/todolist
        [HttpGet]
        public IActionResult Get()
        {
            //
            // The Scope claim tells you what permissions the client application has in the service.
            // In this case we look for a scope value of user_impersonation, or full access to the service as the user.
            //
            //Claim scopeClaim = User.FindFirst("http://schemas.microsoft.com/identity/claims/scope");
            //Claim scopeClaim = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope");
            /*
            if (scopeClaim != null)
            {
                if (scopeClaim.Value != "user_impersonation")
                {
                    return Unauthorized();
                }
            }
            */

            // A user's To Do list is keyed off of the NameIdentifier claim, which contains an immutable, unique identifier for the user.
            //Claim subject = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier);

            todoBag.Add(new TodoItem() { Owner = "Api V1", Title = "title" });

            return Ok(from todo in todoBag
                   //where todo.Owner == subject.Value
                   select todo);
        }

        // POST api/todolist
        [HttpPost]
        public IActionResult Post(TodoItem todo)
        {
            Claim scopeClaim = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope");
            if (scopeClaim != null)
            {
                if (scopeClaim.Value != "user_impersonation")
                {
                    return Unauthorized();
                }
            }

            if (null != todo && !string.IsNullOrWhiteSpace(todo.Title))
            {
                todoBag.Add(new TodoItem { Title = todo.Title, Owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value });
            }

            return Ok();
        }
    }

    [Authorize]
    [ApiVersion("3.0")]
    [ApiVersion("2.0", Deprecated = true)]
    [Route("api/v{version:apiVersion}/TodoList")]
    [ApiController]
    public class TodoListV2Controller : ControllerBase
    {
        //
        // To Do items list for all users.  Since the list is stored in memory, it will go away if the service is cycled.
        //
        static ConcurrentBag<TodoItem> todoBag = new ConcurrentBag<TodoItem>();

        // GET api/todolist
        [HttpGet]
        public IActionResult Get()
        {
            //
            // The Scope claim tells you what permissions the client application has in the service.
            // In this case we look for a scope value of user_impersonation, or full access to the service as the user.
            //
            //Claim scopeClaim = User.FindFirst("http://schemas.microsoft.com/identity/claims/scope");
            //Claim scopeClaim = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope");
            /*
            if (scopeClaim != null)
            {
                if (scopeClaim.Value != "user_impersonation")
                {
                    return Unauthorized();
                }
            }
            */

            // A user's To Do list is keyed off of the NameIdentifier claim, which contains an immutable, unique identifier for the user.
            //Claim subject = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier);

            todoBag.Add(new TodoItem() { Owner = "Api V2", Title = "title" });

            return Ok(from todo in todoBag
                          //where todo.Owner == subject.Value
                      select todo);
        }

        // POST api/todolist
        [HttpPost]
        public IActionResult Post(TodoItem todo)
        {
            Claim scopeClaim = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope");
            if (scopeClaim != null)
            {
                if (scopeClaim.Value != "user_impersonation")
                {
                    return Unauthorized();
                }
            }

            if (null != todo && !string.IsNullOrWhiteSpace(todo.Title))
            {
                todoBag.Add(new TodoItem { Title = todo.Title, Owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value });
            }

            return Ok();
        }
    }
}