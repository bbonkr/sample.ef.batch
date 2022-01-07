using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Sample.Entities;

namespace Sample.Data.EntityTypeConfigurations
{
    public class UserTokenTypeConfiguration : IEntityTypeConfiguration<UserToken>
    {
        public void Configure(EntityTypeBuilder<UserToken> builder)
        {
            builder.ToTable(nameof(UserToken));

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .IsRequired();

            builder.Property(x => x.Token)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();
            builder.Property(x => x.ExpiresAt)
                .IsRequired();
            builder.Property(x => x.Purpose)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
