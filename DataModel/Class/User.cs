using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using DataModel.Enum;
using System.ComponentModel.DataAnnotations;

namespace DataModel.Class
{
	public class User
	{
		public long ID { get; set; }
		[Required]
		public string Username { get; set; }

		/// <summary>
		/// The hash of the user's password.
		/// Computed by hashing the given password after appending the salt.
		/// </summary>
		public byte[] PasswordHash { get; set; }
		public byte[] PasswordSalt { get; set; }

		[Required]
		public string RealName { get; set; }
		[Required]
		public string EmailAddress { get; set; }

		public DateTime LastLogin { get; set; }

		public UserAccessGroup AccessGroup { get; set; }
	}
}
