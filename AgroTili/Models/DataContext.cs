using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgroTili.Models;


namespace AgroTili.Models
{
	public class DataContext : DbContext
	{
		public DataContext(DbContextOptions<DataContext> options) : base(options)
		{

		}
		public DbSet<Empleados> Empleados { get; set; }
		public DbSet<Maquinas_Agrarias> Maquinas_Agrarias { get; set; }
		public DbSet<Roles> Roles { get; set; }
		public DbSet<Tareas> Tareas { get; set; }
        public DbSet<Tipos_Tareas> Tipos_Tareas { get; set; }
        public DbSet<Campos> Campos { get; set; }
		
	}
}