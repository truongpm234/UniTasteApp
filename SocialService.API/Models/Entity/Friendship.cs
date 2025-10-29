    using System;
    using System.Collections.Generic;

    namespace SocialService.API.Models.Entity;

    public partial class Friendship
    {
        public int FriendshipId { get; set; }

        public int? UserId1 { get; set; }

        public int? UserId2 { get; set; }

        public string? Status { get; set; }

        public DateTime? RequestedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
}
