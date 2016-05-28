using System.Collections.Generic;
using Org.Apache.Hadoop.FS;
using Org.Apache.Hadoop.FS.Permission;
using Sharpen;

namespace Org.Apache.Hadoop.Hdfs.Inotify
{
	/// <summary>Events sent by the inotify system.</summary>
	/// <remarks>
	/// Events sent by the inotify system. Note that no events are necessarily sent
	/// when a file is opened for read (although a MetadataUpdateEvent will be sent
	/// if the atime is updated).
	/// </remarks>
	public abstract class Event
	{
		public enum EventType
		{
			Create,
			Close,
			Append,
			Rename,
			Metadata,
			Unlink
		}

		private Event.EventType eventType;

		public virtual Event.EventType GetEventType()
		{
			return eventType;
		}

		public Event(Event.EventType eventType)
		{
			this.eventType = eventType;
		}

		/// <summary>Sent when a file is closed after append or create.</summary>
		public class CloseEvent : Event
		{
			private string path;

			private long fileSize;

			private long timestamp;

			public CloseEvent(string path, long fileSize, long timestamp)
				: base(Event.EventType.Close)
			{
				this.path = path;
				this.fileSize = fileSize;
				this.timestamp = timestamp;
			}

			public virtual string GetPath()
			{
				return path;
			}

			/// <summary>The size of the closed file in bytes.</summary>
			/// <remarks>
			/// The size of the closed file in bytes. May be -1 if the size is not
			/// available (e.g. in the case of a close generated by a concat operation).
			/// </remarks>
			public virtual long GetFileSize()
			{
				return fileSize;
			}

			/// <summary>The time when this event occurred, in milliseconds since the epoch.</summary>
			public virtual long GetTimestamp()
			{
				return timestamp;
			}
		}

		/// <summary>Sent when a new file is created (including overwrite).</summary>
		public class CreateEvent : Event
		{
			public enum INodeType
			{
				File,
				Directory,
				Symlink
			}

			private Event.CreateEvent.INodeType iNodeType;

			private string path;

			private long ctime;

			private int replication;

			private string ownerName;

			private string groupName;

			private FsPermission perms;

			private string symlinkTarget;

			private bool overwrite;

			private long defaultBlockSize;

			public class Builder
			{
				private Event.CreateEvent.INodeType iNodeType;

				private string path;

				private long ctime;

				private int replication;

				private string ownerName;

				private string groupName;

				private FsPermission perms;

				private string symlinkTarget;

				private bool overwrite;

				private long defaultBlockSize = 0;

				public virtual Event.CreateEvent.Builder INodeType(Event.CreateEvent.INodeType type
					)
				{
					this.iNodeType = type;
					return this;
				}

				public virtual Event.CreateEvent.Builder Path(string path)
				{
					this.path = path;
					return this;
				}

				public virtual Event.CreateEvent.Builder Ctime(long ctime)
				{
					this.ctime = ctime;
					return this;
				}

				public virtual Event.CreateEvent.Builder Replication(int replication)
				{
					this.replication = replication;
					return this;
				}

				public virtual Event.CreateEvent.Builder OwnerName(string ownerName)
				{
					this.ownerName = ownerName;
					return this;
				}

				public virtual Event.CreateEvent.Builder GroupName(string groupName)
				{
					this.groupName = groupName;
					return this;
				}

				public virtual Event.CreateEvent.Builder Perms(FsPermission perms)
				{
					this.perms = perms;
					return this;
				}

				public virtual Event.CreateEvent.Builder SymlinkTarget(string symlinkTarget)
				{
					this.symlinkTarget = symlinkTarget;
					return this;
				}

				public virtual Event.CreateEvent.Builder Overwrite(bool overwrite)
				{
					this.overwrite = overwrite;
					return this;
				}

				public virtual Event.CreateEvent.Builder DefaultBlockSize(long defaultBlockSize)
				{
					this.defaultBlockSize = defaultBlockSize;
					return this;
				}

				public virtual Event.CreateEvent Build()
				{
					return new Event.CreateEvent(this);
				}
			}

			private CreateEvent(Event.CreateEvent.Builder b)
				: base(Event.EventType.Create)
			{
				this.iNodeType = b.iNodeType;
				this.path = b.path;
				this.ctime = b.ctime;
				this.replication = b.replication;
				this.ownerName = b.ownerName;
				this.groupName = b.groupName;
				this.perms = b.perms;
				this.symlinkTarget = b.symlinkTarget;
				this.overwrite = b.overwrite;
				this.defaultBlockSize = b.defaultBlockSize;
			}

			public virtual Event.CreateEvent.INodeType GetiNodeType()
			{
				return iNodeType;
			}

			public virtual string GetPath()
			{
				return path;
			}

			/// <summary>Creation time of the file, directory, or symlink.</summary>
			public virtual long GetCtime()
			{
				return ctime;
			}

			/// <summary>Replication is zero if the CreateEvent iNodeType is directory or symlink.
			/// 	</summary>
			public virtual int GetReplication()
			{
				return replication;
			}

			public virtual string GetOwnerName()
			{
				return ownerName;
			}

			public virtual string GetGroupName()
			{
				return groupName;
			}

			public virtual FsPermission GetPerms()
			{
				return perms;
			}

			/// <summary>Symlink target is null if the CreateEvent iNodeType is not symlink.</summary>
			public virtual string GetSymlinkTarget()
			{
				return symlinkTarget;
			}

			public virtual bool GetOverwrite()
			{
				return overwrite;
			}

			public virtual long GetDefaultBlockSize()
			{
				return defaultBlockSize;
			}
		}

		/// <summary>
		/// Sent when there is an update to directory or file (none of the metadata
		/// tracked here applies to symlinks) that is not associated with another
		/// inotify event.
		/// </summary>
		/// <remarks>
		/// Sent when there is an update to directory or file (none of the metadata
		/// tracked here applies to symlinks) that is not associated with another
		/// inotify event. The tracked metadata includes atime/mtime, replication,
		/// owner/group, permissions, ACLs, and XAttributes. Fields not relevant to the
		/// metadataType of the MetadataUpdateEvent will be null or will have their default
		/// values.
		/// </remarks>
		public class MetadataUpdateEvent : Event
		{
			public enum MetadataType
			{
				Times,
				Replication,
				Owner,
				Perms,
				Acls,
				Xattrs
			}

			private string path;

			private Event.MetadataUpdateEvent.MetadataType metadataType;

			private long mtime;

			private long atime;

			private int replication;

			private string ownerName;

			private string groupName;

			private FsPermission perms;

			private IList<AclEntry> acls;

			private IList<XAttr> xAttrs;

			private bool xAttrsRemoved;

			public class Builder
			{
				private string path;

				private Event.MetadataUpdateEvent.MetadataType metadataType;

				private long mtime;

				private long atime;

				private int replication;

				private string ownerName;

				private string groupName;

				private FsPermission perms;

				private IList<AclEntry> acls;

				private IList<XAttr> xAttrs;

				private bool xAttrsRemoved;

				public virtual Event.MetadataUpdateEvent.Builder Path(string path)
				{
					this.path = path;
					return this;
				}

				public virtual Event.MetadataUpdateEvent.Builder MetadataType(Event.MetadataUpdateEvent.MetadataType
					 type)
				{
					this.metadataType = type;
					return this;
				}

				public virtual Event.MetadataUpdateEvent.Builder Mtime(long mtime)
				{
					this.mtime = mtime;
					return this;
				}

				public virtual Event.MetadataUpdateEvent.Builder Atime(long atime)
				{
					this.atime = atime;
					return this;
				}

				public virtual Event.MetadataUpdateEvent.Builder Replication(int replication)
				{
					this.replication = replication;
					return this;
				}

				public virtual Event.MetadataUpdateEvent.Builder OwnerName(string ownerName)
				{
					this.ownerName = ownerName;
					return this;
				}

				public virtual Event.MetadataUpdateEvent.Builder GroupName(string groupName)
				{
					this.groupName = groupName;
					return this;
				}

				public virtual Event.MetadataUpdateEvent.Builder Perms(FsPermission perms)
				{
					this.perms = perms;
					return this;
				}

				public virtual Event.MetadataUpdateEvent.Builder Acls(IList<AclEntry> acls)
				{
					this.acls = acls;
					return this;
				}

				public virtual Event.MetadataUpdateEvent.Builder XAttrs(IList<XAttr> xAttrs)
				{
					this.xAttrs = xAttrs;
					return this;
				}

				public virtual Event.MetadataUpdateEvent.Builder XAttrsRemoved(bool xAttrsRemoved
					)
				{
					this.xAttrsRemoved = xAttrsRemoved;
					return this;
				}

				public virtual Event.MetadataUpdateEvent Build()
				{
					return new Event.MetadataUpdateEvent(this);
				}
			}

			private MetadataUpdateEvent(Event.MetadataUpdateEvent.Builder b)
				: base(Event.EventType.Metadata)
			{
				this.path = b.path;
				this.metadataType = b.metadataType;
				this.mtime = b.mtime;
				this.atime = b.atime;
				this.replication = b.replication;
				this.ownerName = b.ownerName;
				this.groupName = b.groupName;
				this.perms = b.perms;
				this.acls = b.acls;
				this.xAttrs = b.xAttrs;
				this.xAttrsRemoved = b.xAttrsRemoved;
			}

			public virtual string GetPath()
			{
				return path;
			}

			public virtual Event.MetadataUpdateEvent.MetadataType GetMetadataType()
			{
				return metadataType;
			}

			public virtual long GetMtime()
			{
				return mtime;
			}

			public virtual long GetAtime()
			{
				return atime;
			}

			public virtual int GetReplication()
			{
				return replication;
			}

			public virtual string GetOwnerName()
			{
				return ownerName;
			}

			public virtual string GetGroupName()
			{
				return groupName;
			}

			public virtual FsPermission GetPerms()
			{
				return perms;
			}

			/// <summary>The full set of ACLs currently associated with this file or directory.</summary>
			/// <remarks>
			/// The full set of ACLs currently associated with this file or directory.
			/// May be null if all ACLs were removed.
			/// </remarks>
			public virtual IList<AclEntry> GetAcls()
			{
				return acls;
			}

			public virtual IList<XAttr> GetxAttrs()
			{
				return xAttrs;
			}

			/// <summary>
			/// Whether the xAttrs returned by getxAttrs() were removed (as opposed to
			/// added).
			/// </summary>
			public virtual bool IsxAttrsRemoved()
			{
				return xAttrsRemoved;
			}
		}

		/// <summary>Sent when a file, directory, or symlink is renamed.</summary>
		public class RenameEvent : Event
		{
			private string srcPath;

			private string dstPath;

			private long timestamp;

			public class Builder
			{
				private string srcPath;

				private string dstPath;

				private long timestamp;

				public virtual Event.RenameEvent.Builder SrcPath(string srcPath)
				{
					this.srcPath = srcPath;
					return this;
				}

				public virtual Event.RenameEvent.Builder DstPath(string dstPath)
				{
					this.dstPath = dstPath;
					return this;
				}

				public virtual Event.RenameEvent.Builder Timestamp(long timestamp)
				{
					this.timestamp = timestamp;
					return this;
				}

				public virtual Event.RenameEvent Build()
				{
					return new Event.RenameEvent(this);
				}
			}

			private RenameEvent(Event.RenameEvent.Builder builder)
				: base(Event.EventType.Rename)
			{
				this.srcPath = builder.srcPath;
				this.dstPath = builder.dstPath;
				this.timestamp = builder.timestamp;
			}

			public virtual string GetSrcPath()
			{
				return srcPath;
			}

			public virtual string GetDstPath()
			{
				return dstPath;
			}

			/// <summary>The time when this event occurred, in milliseconds since the epoch.</summary>
			public virtual long GetTimestamp()
			{
				return timestamp;
			}
		}

		/// <summary>Sent when an existing file is opened for append.</summary>
		public class AppendEvent : Event
		{
			private string path;

			private bool newBlock;

			public class Builder
			{
				private string path;

				private bool newBlock;

				public virtual Event.AppendEvent.Builder Path(string path)
				{
					this.path = path;
					return this;
				}

				public virtual Event.AppendEvent.Builder NewBlock(bool newBlock)
				{
					this.newBlock = newBlock;
					return this;
				}

				public virtual Event.AppendEvent Build()
				{
					return new Event.AppendEvent(this);
				}
			}

			private AppendEvent(Event.AppendEvent.Builder b)
				: base(Event.EventType.Append)
			{
				this.path = b.path;
				this.newBlock = b.newBlock;
			}

			public virtual string GetPath()
			{
				return path;
			}

			public virtual bool ToNewBlock()
			{
				return newBlock;
			}
		}

		/// <summary>Sent when a file, directory, or symlink is deleted.</summary>
		public class UnlinkEvent : Event
		{
			private string path;

			private long timestamp;

			public class Builder
			{
				private string path;

				private long timestamp;

				public virtual Event.UnlinkEvent.Builder Path(string path)
				{
					this.path = path;
					return this;
				}

				public virtual Event.UnlinkEvent.Builder Timestamp(long timestamp)
				{
					this.timestamp = timestamp;
					return this;
				}

				public virtual Event.UnlinkEvent Build()
				{
					return new Event.UnlinkEvent(this);
				}
			}

			private UnlinkEvent(Event.UnlinkEvent.Builder builder)
				: base(Event.EventType.Unlink)
			{
				this.path = builder.path;
				this.timestamp = builder.timestamp;
			}

			public virtual string GetPath()
			{
				return path;
			}

			/// <summary>The time when this event occurred, in milliseconds since the epoch.</summary>
			public virtual long GetTimestamp()
			{
				return timestamp;
			}
		}
	}
}