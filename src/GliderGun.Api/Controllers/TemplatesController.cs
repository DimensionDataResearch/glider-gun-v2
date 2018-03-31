using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GliderGun.Api.Controllers
{
    using Data;
    using Data.Models;

    /// <summary>
    ///     The controller for the Glider Gun templates API.
    /// </summary>
    [Route("api/v2/templates")]
    public class TemplatesController
        : Controller
    {
        /// <summary>
        ///     Create a new <see cref="TemplatesController"/>.
        /// </summary>
        /// <param name="data">
        ///     The Glider Gun database context.
        /// </param>
        public TemplatesController(DataContext data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            Data = data;
        }

        /// <summary>
        ///     The Glider Gun database context.
        /// </summary>
        DataContext Data { get; }

        /// <summary>
        ///     List available templates.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> List()
        {
            Template[] templates = await Data.Templates.ToArrayAsync();

            return Ok(templates);
        }
    }
}
