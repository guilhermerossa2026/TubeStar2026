using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace TubeStar
{
    [Serializable]
    public enum EnglishStrings
    {
        [Description("Fails")]
        Fails,

        [Description("Animal fighting")]
        AnimalFighting,

        [Description("Anime")]
        Anime,

        [Description("Cats")]
        Cats,

        [Description("Comedy")]
        Comedy,

        [Description("Conspiracy theory")]
        ConspiraryTheory,

        [Description("Creepypasta")]
        CreepyPasta,

        [Description("Gaming")]
        Gaming,

        [Description("Shopping haul")]
        ShoppingHaul,

        [Description("How to ...")]
        HowTo,

        [Description("Movies")]
        Movies,

        [Description("Music video")]
        MusicVideo,

        [Description("Non profit")]
        NonProfit,

        [Description("Sports")]
        Sports,

        [Description("The weird side")]
        TheWeirdSide,

        [Description("Technology")]
        Technology,

        [Description("Ukulele cover")]
        UkuleleCover,

        [Description("Vlog")]
        Vlog,

        [Description("OK")]
        Ok,

        [Description("Cancel")]
        Cancel,

        [Description("Something went horribly wrong!\nOr maybe only slightly wrong...\nAnyway, sorry! I couldn't save the game!")]
        SaveFail,

        [Description("Something went horribly wrong!\nOr maybe only slightly wrong...\nAnyway, sorry! I couldn't load the game!")]
        LoadFail,

        [Description("Channel '{0}' has been suspended due to a suspected violation of the terms and conditions!")]
        ChannelSuspended,

        [Description("Your computer has crashed!\n\nAll unreleased videos have been lost!")]
        ComputerCrash,

        [Description("The economy is failing!\n\nYour cost of living has increased!")]
        CostOfLivingIncrease,

        [Description("Your work has been outsourced!\n\nYou're fired!")]
        Fired,

        [Description("Some embarrassing information about you has surfaced!\n\nYou've lost {0} subscribers!")]
        LoseSubscribers,

        [Description("Mmm, yeah, I'm going to have to go ahead and ask you to work overtime.\n\nSo if you could go ahead and do that, that'd be great!")]
        WorkOvertime,

        [Description("Video '{0}' has been suspended due to suspected copyright infringement!")]
        VideoSuspended,

        [Description("You searched through your sofa and found {0} change")]
        SpareChange,

        [Description("You've been promoted at work!\n\nYou earn more money while working longer hours!")]
        Promoted,

        [Description("The robots have risen and taken over the earth!\n\nBow down to your new robot rulers.")]
        RobotRising,

        [Description("You've won {0} in a beauty contest!")]
        BeautyContest,

        [Description("Channel '{0}' has had it's suspension lifted!")]
        ChannelSuspensionLifted,

        [Description("The economy is recovering!\n\nYour cost of living has decreased!")]
        CostOfLivingDecreased,

        [Description("You've been given a scholarship to {0}!")]
        FreeStudy,

        [Description("You've been featured on a website!\n\nYou've gained {0} subscribers!")]
        GainSubscribers,

        [Description("A video '{0}' has had it's suspension lifted!")]
        VideoSuspensionLifted,

        [Description("Random event")]
        RandomEvent,

        [Description("Study 'Video Attribution Technology I'")]
        StudyAudienceAnalysis1,

        [Description("Study 'Video Attribution Technology II'")]
        StudyAudienceAnalysis2,

        [Description("Study 'Post Production I'")]
        StudyPostProduction1,

        [Description("Study 'Post Production II'")]
        StudyPostProduction2,

        [Description("Study 'Post Production III'")]
        StudyPostProduction3,

        [Description("Study 'Production I'")]
        StudyProduction1,

        [Description("Study 'Production II'")]
        StudyProduction2,

        [Description("Study 'Production III'")]
        StudyProduction3,

        [Description("Study 'Quality Analysis'")]
        StudyQualityAnalysis,

        [Description("Mandatory Bowing to Robot Rulers")]
        MandatoryBowToRobotRulers,

        [Description("'{0}' post production")]
        EditVideoTask,

        [Description("Job")]
        Job,

        [Description("'{0}' video shoot")]
        ShootVideoTask,

        [Description("May contain cats")]
        CatsAttribute,

        [Description("Video quality increased slightly")]
        CatsAttributeDescription,

        [Description("Copycat")]
        CopycatAttribute,

        [Description("{0} free initial views")]
        CopycatAttributeDescription,

        [Description("Fanboy bait")]
        FanboyBaitAttribute,

        [Description("If the video quality is over 75%, no current subscribers will unsubscribe")]
        FanboyBaitAttributeDescription,

        [Description("Genre buster")]
        GenreBusterAttribute,

        [Description("Video category is less of a factor in likeability")]
        GenreBusterAttributeDescription,

        [Description("Hypnotic")]
        HypnoticAttribute,

        [Description("Higher chance of subscriptions")]
        HypnoticAttributeDescription,

        [Description("Learning from mistakes")]
        LearnFromMistakesAttribute,

        [Description("Player stats slightly increased at cost of video quality")]
        LearnFromMistakesAttributeDescription,

        [Description("Memetic")]
        MemeticAttribute,

        [Description("Higher chance of sharing")]
        MemeticAttributeDescription,

        [Description("Product Placement")]
        ProductPlacementAttribute,

        [Description("Earn more from advertising per view")]
        ProductPlacementAttributeDescription,

        [Description("Better second time")]
        SecondTimeAttribute,

        [Description("The video can be viewed multiple times")]
        SecondTimeAttributeDescription,

        [Description("So bad it's good")]
        SoBadAttribute,

        [Description("Chance of video being shared if the video is disliked")]
        SoBadAttributeDescription,

        [Description("Crowdfunding")]
        CrowdFundingAttribute,

        [Description("Instead of a subscription you get {0}")]
        CrowdFundingAttributeDescription,

        [Description("Above board")]
        AboveBoardAttribute,

        [Description("The video cannot be suspended")]
        AboveBoardAttributeDescription,

        [Description("Unreleased Videos")]
        UnreleasedVideos,

        [Description("Continue game")]
        ContinueGame,

        [Description("New game")]
        NewGame,

        [Description("Tutorial")]
        Tutorial,

        [Description("Credits")]
        Credits,

        [Description("What a loser")]
        WhatALoser,

        [Description("You ran out of money!")]
        OutOfMoney,

        [Description("Death")]
        Death,

        [Description("You died fighting the robot menace.\nYour official cause of death is noted as:\n\n'Exhaustion from overeagerness to bow to robot masters.'")]
        RobotDeath,

        [Description("Confirm")]
        Confirm,

        [Description("Leave the current game?")]
        LeaveGame,

        [Description("Overwrite save")]
        OverwriteSave,

        [Description("A save file already exists!\nOverwrite?")]
        SaveExists,

        [Description("Start day")]
        StartDay,

        [Description("Help")]
        Help,

        [Description("Exit")]
        Exit,

        [Description("Save Game")]
        SaveGame,

        [Description("Unhandled Error")]
        UnhandledError,

        [Description("An unknown error has occured!\n\nError report created in the games folder.\n\nClick OK to continue, CANCEL to quit")]
        ExceptionText,

        [Description("Aren't you missing something?")]
        MissingValueHeader,

        [Description("Enter a name")]
        MissingName,

        [Description("Select a category")]
        MissingCategory,

        [Description("Select a channel")]
        MissingChannel,

        [Description("Select a strategy")]
        MissingStrategy,

        [Description("Select a video")]
        MissingVideo,

        [Description("Enter the username")]
        MissingUserName,

        [Description("Enter the token")]
        MissingToken,

        [Description("per view")]
        PerView,

        [Description("Add channel")]
        AddChannel,

        [Description("Edit channel")]
        EditChannel,

        [Description("Ads in videos")]
        AdvertisingStrategy,

        [Description("Name")]
        Name,

        [Description("Low")]
        Low,

        [Description("Normal")]
        Normal,

        [Description("High")]
        High,

        [Description("Aggressive")]
        Aggressive,

        [Description("Shoot video")]
        ShootVideo,

        [Description("Edit video")]
        EditVideo,

        [Description("Study")]
        Study,

        [Description("Jumped the gun")]
        TooSoon,

        [Description("There are no videos to edit!")]
        NoVideosToEdit,

        [Description("points left")]
        PointsLeft,

        [Description("How you gonna pay for that?")]
        LowCashHeader,

        [Description("Not enough cash!")]
        LowCashMessage,

        [Description("Hours")]
        Hours,

        [Description("Hours left: {0}")]
        HoursLeft,

        [Description("Add video")]
        AddVideo,

        [Description("Attributes")]
        Attributes,

        [Description("Cost")]
        Cost,

        [Description("Category")]
        Category,

        [Description("Suspended")]
        Suspended,

        [Description("Video")]
        Video,

        [Description("Videos")]
        Videos,

        [Description("Add Task")]
        AddTask,

        [Description("Quit your job")]
        QuitJobHeader,

        [Description("Taking this action will quit your job for good. You will not be able to get it back.\nAre you sure?")]
        QuitJobText,

        [Description("Part-time")]
        PartTimeButton,

        [Description("Full-time")]
        FullTimeButton,

        [Description("You can change again in {0} day(s)")]
        ShiftChangeWait,

        [Description("Change work shift")]
        ChangeShiftHeader,

        [Description("Switch to part-time?\nYou will work 4 hours a day and earn $50 a day (instead of 9 hours and $100).\nIt starts tomorrow, and you can only change again after 7 days.")]
        PartTimeText,

        [Description("Switch to full-time?\nYou will work 9 hours a day and earn $100 a day (instead of 4 hours and $50).\nIt starts tomorrow, and you can only change again after 7 days.")]
        FullTimeText,

        [Description("Rise up!")]
        RiseUp,

        [Description("You need at least {0} to start a rebellion against our robot masters!")]
        RebellionCashRequired,

        [Description("Taking this action will start an uprising against our robot masters!\nYou have a {0}% chance of success and failure will result in DEATH! \n\nAre you sure?")]
        RebellionStart,

        [Description("Freedom!")]
        Freedom,

        [Description("You find the master switch and switch the master robot race off!\nThat was easy!")]
        RebellionSuccess,

        [Description("Remove task")]
        RemoveTask,

        [Description("Are you sure?")]
        AreYouSure,

        [Description("No refunds!")]
        NoRefunds,

        [Description("Casual")]
        Casual,

        [Description("Challenge")]
        Challenge,

        [Description("Failure")]
        Failure,

        [Description("User name")]
        Username,

        [Description("Token")]
        Token,

        [Description("Hours to complete")]
        HoursToComplete,

        [Description("Prerequisites not met!")]
        PrerequisitesNotMet,

        [Description("Already completed")]
        AlreadyCompleted,

        [Description("Already enrolled")]
        AlreadyEnrolled,

        [Description("Study selection")]
        StudySelection,

        [Description("Upload video")]
        UploadVideo,

        [Description("Random image")]
        FetchNewImage,

        [Description("Image")]
        Image,

        [Description("Buy views")]
        BuyViews,

        [Description("Channel")]
        Channel,

        [Description("Views")]
        Views,

        [Description("None")]
        None,

        [Description("Comments")]
        Comments,

        [Description("Quality")]
        Quality,

        [Description("N/A")]
        NotApplicable,

        [Description("Likes")]
        Likes,

        [Description("Dislikes")]
        Dislikes,

        [Description("Subscribers")]
        Subscribers,

        [Description("Channel Income")]
        ChannelIncome,

        [Description("Where to?")]
        WhereTo,

        [Description("Create a channel to upload to!")]
        ChannelNeeded,

        [Description("Video Manager")]
        VideoManager,

        [Description("Daily Planner")]
        DailyPlanner,

        [Description("Stats")]
        Stats,

        [Description("Subscriber History")]
        SubscriberHistory,

        [Description("Daily Income and Expenses")]
        DailyIncome,

        [Description("Generic Camcorder XYZ-85")]
        VideoCameraIStoreItem,

        [Description("It's better than a webcam...")]
        VideoCameraIStoreItemDescription,

        [Description("Super Camcorder Pro Deluxe HD")]
        VideoCameraIIStoreItem,

        [Description("Best video camera on the market! Used by all the pros!")]
        VideoCameraIIStoreItemDescription,

        [Description("Edit-Me Video Director 4")]
        EditingSoftwareIStoreItem,

        [Description("Better than the one that comes free with my OS...")]
        EditingSoftwareIStoreItemDescription,

        [Description("Mega-Director Ultra Deluxe Edition 12")]
        EditingSoftwareIIStoreItem,

        [Description("The ultimate in video editing software! 20 different star swipes to choose from!")]
        EditingSoftwareIIStoreItemDescription,

        [Description("Cinema Camera Pro 4K")]
        VideoCameraIIIStoreItem,

        [Description("Shoots like a real film set. Your videos will look expensive.")]
        VideoCameraIIIStoreItemDescription,

        [Description("Studio Cinema Rig 6K")]
        VideoCameraIVStoreItem,

        [Description("A full studio rig with lenses, lights and a crew-sized price tag.")]
        VideoCameraIVStoreItemDescription,

        [Description("Hollywood Cinema Rig 8K")]
        VideoCameraVStoreItem,

        [Description("The same camera the blockbusters use. Overkill? Never.")]
        VideoCameraVStoreItemDescription,

        [Description("Cut-Master Studio Pro")]
        EditingSoftwareIIIStoreItem,

        [Description("Pro-grade timeline, color grading and sound mixing.")]
        EditingSoftwareIIIStoreItemDescription,

        [Description("Hollywood Suite Ultra")]
        EditingSoftwareIVStoreItem,

        [Description("The suite real editors fight over. Effects for everything.")]
        EditingSoftwareIVStoreItemDescription,

        [Description("Blockbuster Studio Infinite")]
        EditingSoftwareVStoreItem,

        [Description("Unlimited tracks, unlimited effects, almost unlimited price.")]
        EditingSoftwareVStoreItemDescription,

        [Description("Wright & Wrong Attorneys")]
        LawyerStoreItem,

        [Description("For all your legal needs! We'll unblock any suspended video or channel for a small fee of {0}!")]
        LawyerStoreItemDescription,

        [Description("Bob Boskins - Consultant")]
        ConsultantStoreItem,

        [Description("With my expert guidance, I'll make you a TubeStar! I also charge {0} per video!")]
        ConsultantStoreItemDescription,

        [Description("Buy")]
        Buy,

        [Description("Online Store")]
        OnlineStore,

        [Description("(Click to Hire Lawyer)")]
        SuspendedHireLawyer,

        [Description("Do you want to pay our fee of {0} to lift this video's suspension?")]
        RemoveVideoSuspension,

        [Description("Do you want to pay our fee of {0} to lift this channel's suspension?")]
        RemoveChannelSuspension,

        [Description("Shylock's Loans")]
        LoanStoreItem,

        [Description("Immediate payout of {0} guaranteed!! Call today! \nPay back {1} per day. {2}% interest.")]
        LoanStoreItemDescription,

        [Description("Edit")]
        Edit,

        [Description("Save")]
        Save,

        [Description("Add Translation")]
        AddTranslation,

        [Description("View Video")]
        ViewVideo,

        [Description("Born to Rule")]
        BornToRule,

        [Description("Buy for {0}?")]
        BuyItem,

        [Description("Requires: {0}")]
        RequiresItem,

        [Description("Next")]
        Next,

        [Description("The game was successfully saved!")]
        SaveGameSuccess,

        [Description("The robots take 20% of your money in tribute to their glory.")]
        RobotTax,

        [Description("Top TubeStars")]
        TopTubeStars,

        [Description("Ultra")]
        Ultra,

        [Description("Delete")]
        Delete,

        [Description("Delete Video")]
        DeleteVideo,

        [Description("Display 'Creative Commons' Material Only")]
        UseCreativeCommons,

        [Description("Home")]
        NavHome,

        [Description("Schedule")]
        NavSchedule,

        [Description("My videos")]
        NavMyVideos,

        [Description("Create video")]
        NavCreateVideo,

        [Description("Post video")]
        NavPostVideo,

        [Description("Store")]
        NavStore,

        [Description("University")]
        NavUniversity,

        [Description("Employees")]
        NavEmployees,

        [Description("Soon")]
        Soon,

        [Description("Next day")]
        NextDay,

        [Description("Money")]
        MoneyLabel,

        [Description("Year {0} · Week {1} · Day {2}")]
        GameDate,

        [Description("You have {0}.")]
        YouHave,

        [Description("Purchased")]
        Purchased,

        [Description("You")]
        You,

        [Description("Back to Top")]
        BackToTop,

        [Description("Ranking of the channels in the game world. Climb positions by posting often and with quality.")]
        TopTubeStarsLead,

        [Description("Distribute the hours of the day. When you click Next day, the tasks move forward.")]
        ScheduleLead,

        [Description("Hours of the day (15h)")]
        ScheduleDayHours,

        [Description("{0} of 15 hours used")]
        ScheduleHoursUsed,

        [Description("{0}h today")]
        ScheduleHoursToday,

        [Description("Work shift booked for today")]
        ScheduleJobStatus,

        [Description("Completed")]
        ScheduleTaskDone,

        [Description("{0}h left")]
        ScheduleHoursRemaining,

        [Description("Remove")]
        ScheduleRemove,

        [Description("Work, shoot, edit, study")]
        ScheduleAddTaskHint,

        [Description("The score (from 0 to 10) decides how many views the video gets per day.")]
        VideosLead,

        [Description("Not released")]
        VideosUnreleasedChip,

        [Description("+ New channel")]
        VideosNewChannel,

        [Description("Most recent")]
        VideosFilterRecent,

        [Description("Most viewed")]
        VideosFilterMostViewed,

        [Description("Best score")]
        VideosFilterBestScore,

        [Description("Search video")]
        VideosSearch,

        [Description("View all")]
        VideosViewAll,

        [Description("View by page")]
        VideosViewPages,

        [Description("‹ Previous")]
        VideosPrevious,

        [Description("Next ›")]
        VideosNext,

        [Description("Page {0} of {1}")]
        VideosPageOf,

        [Description("Score {0}")]
        VideosScore,

        [Description("{0} views")]
        VideosViewsCount,

        [Description("Panel: {0}")]
        VideosPanel,

        [Description("Total views")]
        VideosTotalViews,

        [Description("Total earnings")]
        VideosTotalEarnings,

        [Description("Ready videos")]
        VideosReady,

        [Description("Finished videos wait here. Click Post to choose the channel.")]
        VideosUnreleasedHint,

        [Description("Post")]
        VideosPost,

        [Description("Statistics")]
        VideosStatistics,

        [Description("Edit channel")]
        VideosEditChannel,

        [Description("Delete channel")]
        VideosDeleteChannel,

        [Description("Delete channel {0}? All of its videos will be erased ({1} in total). This cannot be undone.")]
        VideosDeleteChannelConfirm,

        [Description("No videos here yet.")]
        VideosNone,

        [Description("Delete")]
        VideosDelete,

        [Description("Pick a title and category, choose the attributes and invest in production.")]
        CreateVideoLead,

        [Description("Summary")]
        CreateVideoSummary,

        [Description("Thumbnail")]
        ThumbnailLabel,

        [Description("Search")]
        ThumbSearch,

        [Description("Use this")]
        ThumbUseThis,

        [Description("Search others")]
        ThumbSearchOthers,

        [Description("{0} of {1}")]
        ThumbCounter,

        [Description("Searching...")]
        ThumbSearching,

        [Description("Loading...")]
        ThumbLoading,

        [Description("No thumbnails found.")]
        ThumbNoResults,

        [Description("Couldn't load thumbnails. Check your internet or the API key.")]
        ThumbOffline,

        [Description("Daily YouTube search limit reached. It resets tomorrow.")]
        ThumbQuotaReached,

        [Description("Searches today: {0}/{1}")]
        ThumbSearchesToday,

        [Description("(limit reached)")]
        ThumbLimitReached,

        [Description("Type the title to see thumbnails.")]
        ThumbTypeTitle,

        [Description("Search: {0}")]
        ThumbQuery,

        [Description("Searches by title; 1 API search for every 50 thumbnails.")]
        ThumbHint,

        [Description("From the library")]
        ThumbFromLibrary,

        [Description("From the library (similar)")]
        ThumbFromLibraryPartial,

        [Description("From search")]
        ThumbFromSearch,
        [Description("From search (saved, no cost)")]
        ThumbFromSearchCached,

        [Description("From the niche (random)")]
        ThumbFromNiche,

        [Description("Search more (API)")]
        ThumbSearchMore,

        [Description("Updating thumbnails… {0}% · channel {1} of {2}")]
        ThumbLibUpdating,

        [Description("Thumbnails ready ✓ ({0} videos)")]
        ThumbLibReady,

        [Description("Thumbnails: couldn't update, using the previous library ({0} videos)")]
        ThumbLibFailed,

        [Description("Library: updating {0}%")]
        ThumbLibShort,

        [Description("Series")]
        SeriesLabel,

        [Description("Continue series")]
        SeriesContinue,

        [Description("{0} · last #{1}")]
        SeriesLast,

        [Description("Remove series from list")]
        SeriesRemove,

        [Description("Remove \"{0}\" from the series list? The videos will not be deleted.")]
        SeriesRemoveText,

        [Description("No series yet. End a title with #1 to start one.")]
        SeriesNone,

        [Description("Thumbnail chosen.")]
        ThumbChosen,

        [Description("No thumbnail chosen: the first result will be used.")]
        ThumbAuto,

        [Description("Pick a ready video, the channel and how to promote it.")]
        PostVideoLead,

        [Description("Ready videos")]
        PostReadyVideos,

        [Description("No video ready. Create and edit a video in the Schedule.")]
        PostNoVideos,

        [Description("Change thumbnail")]
        PostChangeThumb,

        [Description("You have no channel yet. Create one to post your videos.")]
        PostNoChannel,

        [Description("Promote video")]
        PostPromote,

        [Description("Plan and record a new video")]
        AddTaskShootHint,

        [Description("Polish a recorded video")]
        AddTaskEditHint,

        [Description("Take a course and learn new skills")]
        AddTaskStudyHint,

        [Description("Your path from zero to YouTube star")]
        TitleTagline,

        [Description("Yesterday's summary")]
        SummaryYesterday,

        [Description("No day played yet.")]
        SummaryNoDayYet,

        [Description("Your video \"{0}\" went viral! The algorithm is pushing it.")]
        AlertVideoViral,

        [Description("\"{0}\" didn't catch on: the algorithm barely showed it.")]
        AlertVideoFlop,

        [Description("\"{0}\" is doing better than usual.")]
        AlertVideoGood,

        [Description("You lost {0} subscribers because of weak videos.")]
        AlertUnsubsWeak,

        [Description("You lost {0} subscribers for not posting.")]
        AlertUnsubsIdle,

        [Description("Earned")]
        SummaryEarned,

        [Description("Spent")]
        SummarySpent,

        [Description("Balance")]
        SummaryBalance,

        [Description("New subscribers")]
        SummaryNewSubscribers,

        [Description("Videos posted")]
        SummaryVideosPosted,

        [Description("Tasks completed")]
        SummaryTasksDone,

        [Description("Week {0} summary · Year {1}")]
        SummaryWeekTitle,

        [Description("Year {0} summary")]
        SummaryYearTitle,

        [Description("By channel")]
        SummaryByChannel,

        [Description("Best video")]
        SummaryBestVideo,

        [Description("No channel had activity.")]
        SummaryNoChannels,

        [Description("Days online")]
        DaysOnline,
        [Description("{0} days")]
        VideoDaysOnline,
        [Description("Performance")]
        VideoTabPerformance,
        [Description("Money")]
        VideoTabMoney,
        [Description("Why the score")]
        VideoWhyScore,
        [Description("Shooting")]
        VideoShooting,
        [Description("Editing")]
        VideoEditing,
        [Description("not edited")]
        VideoNotEdited,
        [Description("{0} points")]
        VideoPoints,
        [Description("Attributes: {0}")]
        VideoAttributesList,
        [Description("Rank in channel")]
        VideoRankInChannel,
        [Description("{0} of {1}")]
        VideoRankValue,
        [Description("Approval")]
        VideoApproval,
        [Description("Cost")]
        VideoCost,
        [Description("Earned")]
        VideoIncome,
        [Description("Profit")]
        VideoProfit,
        [Description("Profit: {0}")]
        VideoProfitHeader,
        [Description("Earnings include ads and crowdfunding, counted day by day since the video was posted.")]
        VideoMoneyNote,
        [Description("Week")]
        RankWeek,
        [Description("Overview")]
        ChannelOverview,
        [Description("{0} channels")]
        ChannelOverviewInfo,
        [Description("1 channel")]
        ChannelOverviewInfoOne,
        [Description("RPM")]
        ChannelRpm,
        [Description("Ranking")]
        ChannelRank,
        [Description("Subscribers per day")]
        ChannelSubscribersChart,
        [Description("The chart appears after a few days.")]
        ChannelChartEmpty,
        [Description("Total")]
        ChannelTotal,
        [Description("All niches")]
        RankAllNiches,
        [Description("No channel here yet.")]
        RankEmpty,
        [Description("Salary")]
        SummarySalary,
        [Description("Channel")]
        SummaryChannel,
        [Description("Other")]
        SummaryOther,
        [Description("Living cost")]
        SummaryLivingCost,
        [Description("Videos")]
        SummaryVideoCost,
        [Description("Video promotion")]
        SummaryPromotionCost,
        [Description("Loan")]
        SummaryLoanCost,
        [Description("Tax")]
        SummaryTaxCost,
        [Description("Weekly tax paid: {0}")]
        AlertTaxPaid,
        [Description("Tax debt: {0} (charged next week)")]
        AlertTaxDebt,
        [Description("Tax Lawyer")]
        TaxLawyerStoreItem,
        [Description("Cuts the weekly channel profit tax by {0} percentage points in every bracket (15% becomes 10%, 25% becomes 20%).")]
        TaxLawyerStoreItemDescription,
        [Description("Your cost of living went up ${0}/day (now ${1}/day)")]
        CostOfLivingIncreaseAmount,
        [Description("Your cost of living went down ${0}/day (now ${1}/day)")]
        CostOfLivingDecreaseAmount,
        [Description("Subscribers gained")]
        VideoSubscribersGained,
        [Description("Niche")]
        NicheLabel,
        [Description("Choose the channel niche. It cannot be changed later.")]
        NicheChooseHint,
        [Description("RPM: {0} / 1,000 views · Competition: {1}")]
        NicheInfo,
        [Description("High")]
        NicheLevelHigh,
        [Description("Medium")]
        NicheLevelMedium,
        [Description("Low")]
        NicheLevelLow,
        [Description("High")]
        NicheCrowdHigh,
        [Description("Medium")]
        NicheCrowdMedium,
        [Description("Low")]
        NicheCrowdLow,
        [Description("The niche cannot be changed after the channel is created.")]
        NicheFixed,
        [Description("Choose the channel niche.")]
        NicheMissing,
        [Description("This video is outside the channel niche ({0}): fewer subscribers will watch, but more new people may subscribe.")]
        NicheOutWarning,
        [Description("#{0} in the ranking")]
        ProfilePosition,
        [Description("Videos")]
        ProfileVideos,
        [Description("Change since yesterday")]
        ProfileChange,
        [Description("No change")]
        ProfileNoChange,
        [Description("Tied with you")]
        ProfileDistanceTie,
        [Description("Days at #1")]
        ProfileDaysAtTop,
        [Description("Best position: #{0}")]
        ProfileBest,
        [Description("Distance to you: {0} subscribers")]
        ProfileDistanceBehind,
        [Description("You are {0} subscribers ahead")]
        ProfileDistanceAhead,
        [Description("K")]
        NumberThousand,
        [Description("M")]
        NumberMillion,
        [Description("B")]
        NumberBillion,
        [Description("T")]
        NumberTrillion,
        [Description(".")]
        NumberDecimalSeparator,
        [Description("Back to #1")]
        RankGoToTop,
        [Description("Subs this week")]
        RankSubscribersWeek,
        [Description("Views this week")]
        RankViewsWeek,
        [Description("Overall")]
        RankTabAll,
        [Description("Grew this week")]
        RankTabWeek,
        [Description("Go to my position")]
        RankGoToMe,
        [Description("Page {0} of {1}")]
        RankPage,
        [Description("Biggest rise of the week")]
        RankBiggestRise,
        [Description("Biggest fall of the week")]
        RankBiggestFall,
        [Description(" this week")]
        RankThisWeek,
        [Description("Create a channel to join the ranking")]
        RankNoChannel,
        [Description("Games")]
        ThemeGames,
        [Description("Vlog / Lifestyle")]
        ThemeVlog,
        [Description("Technology")]
        ThemeTech,
        [Description("Comedy")]
        ThemeComedy,
        [Description("Education")]
        ThemeEducation,
        [Description("Finance")]
        ThemeFinance,
        [Description("Fitness / Health")]
        ThemeFitness,
        [Description("Cooking")]
        ThemeCooking,
        [Description("Music")]
        ThemeMusic,
        [Description("Beauty / Fashion")]
        ThemeBeauty,
        [Description("Sports")]
        ThemeSports,
        [Description("Reacts")]
        ThemeReacts,
        [Description("Mystery / True Crime")]
        ThemeMystery,
        [Description("Travel")]
        ThemeTravel,
        [Description("Kids")]
        ThemeKids,

        [Description("Calendar")]
        CalendarTitle,

        [Description("Click to open the calendar")]
        CalendarOpenHint,

        [Description("This week so far")]
        CalendarThisWeek,

        [Description("Past summaries")]
        CalendarPastSummaries,

        [Description("No week closed yet. The first summary appears on Day 8.")]
        CalendarNoHistory,

        [Description("Day {0}")]
        CalendarDay,

        [Description("Today")]
        CalendarToday,

        [Description("Video type")]
        TypeLabel,

        [Description("Tutorial")]
        TypeTutorial,

        [Description("Review")]
        TypeReview,

        [Description("React")]
        TypeReact,

        [Description("Challenge")]
        TypeChallenge,

        [Description("Vlog")]
        TypeVlog,

        [Description("Top 10")]
        TypeTop10,

        [Description("Documentary")]
        TypeDocumentary,

        [Description("Gameplay")]
        TypeGameplay,

        [Description("Matches {0}: +10% views")]
        TypeMatches,

        [Description("Doesn't match {0}: no bonus")]
        TypeNoMatch,

        [Description("Matches: {0}")]
        TypeMatchesList,

        [Description("Choose a type. Types that match the video's niche get +10% views.")]
        TypeChooseHint,

        [Description("Choose the video type.")]
        MissingType,

        [Description("Production investment")]
        InvestmentLabel,

        [Description("None")]
        InvestmentNone,

        [Description("Basic")]
        InvestmentBasic,

        [Description("Good")]
        InvestmentGood,

        [Description("Premium")]
        InvestmentPremium,

        [Description("No extra investment: the score depends only on your skills.")]
        InvestmentNoneInfo,

        [Description("Raises the video score by up to +{0}.")]
        InvestmentInfo,

        [Description("Shoot now: {0}h · Then edit: {1}h (add it to the schedule when shooting is done)")]
        HoursFixedInfo,

        [Description("Time drops as your shooting and editing skills grow (Studies).")]
        HoursFixedHint,

        [Description("Small")]
        PromoteSmall,

        [Description("Medium")]
        PromoteMedium,

        [Description("Large")]
        PromoteLarge,

        [Description("No promotion: the video only gets organic views.")]
        PromoteNoneInfo,

        [Description("About {0} views over {1} days")]
        PromoteEstimateViews,

        [Description("{0} to {1} subscribers")]
        PromoteEstimateSubs,

        [Description("Promoted views don't pay ads: they help the channel grow. The better the video, the more subscribers.")]
        PromoteNoAds,

        [Description("Game saved")]
        GameSaved,

        [Description("RPM now: what the channel earns per 1,000 views, today. It depends on the niche, time of year, ads in videos, video quality and subscribers.")]
        ChannelRpmTip,

        [Description("Average so far: {0} per 1,000 views")]
        ChannelRpmAverage,

        [Description("Earned per 1,000 views of this video. RPM now for this channel and video: {0}")]
        VideoRpmTip,

        [Description("Ad season: high")]
        CalendarAdSeasonHigh,

        [Description("Ad season: weak")]
        CalendarAdSeasonLow,

        [Description("Ads up")]
        HeaderAdsUp,

        [Description("Ads weak")]
        HeaderAdsWeak,

        [Description("Advertisers pay more at this time (up to +30% RPM)")]
        HeaderAdsUpTip,

        [Description("Advertisers pay less early in the year (-25% RPM)")]
        HeaderAdsWeakTip,

        [Description("Ads in videos:\nLow: RPM -15%, 90% of likers subscribe\nNormal: RPM +0%, 70% of likers subscribe\nHigh: RPM +10%, 50% of likers subscribe\nAggressive: RPM +20%, 30% of likers subscribe")]
        StrategyTable,
    }
}