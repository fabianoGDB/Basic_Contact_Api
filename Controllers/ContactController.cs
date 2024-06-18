using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ContactsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ContactsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetContacts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = null)
    {
        var query = _context.Contacts.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.Name.Contains(search) || c.Email.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var contacts = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new { totalCount, contacts });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetContact(Guid id)
    {
        var contact = await _context.Contacts.FindAsync(id);

        if (contact == null)
        {
            return NotFound();
        }

        return Ok(contact);
    }

    [HttpPost]
    public async Task<IActionResult> CreateContact([FromBody] Contact contact)
    {
        contact.CreatedAt = DateTime.Now;
        contact.LastUpdatedAt = DateTime.Now;

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetContact), new { id = contact.Id }, contact);
    }

    [HttpPost("bulk-insert")]
    public async Task<IActionResult> BulkInsert([FromBody] List<Contact> contacts)
    {
        _context.Contacts.AddRange(contacts);
        await _context.SaveChangesAsync();
        return Ok(new { result = $"{contacts.Count} contacts inserted successfully!" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContact(Guid id, [FromBody] Contact contact)
    {
        contact.LastUpdatedAt = DateTime.Now;

        if (id != contact.Id)
        {
            return BadRequest();
        }

        _context.Entry(contact).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ContactExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContact(Guid id)
    {
        var contact = await _context.Contacts.FindAsync(id);
        if (contact == null)
        {
            return NotFound();
        }

        _context.Contacts.Remove(contact);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ContactExists(Guid id)
    {
        return _context.Contacts.Any(e => e.Id == id);
    }
}
