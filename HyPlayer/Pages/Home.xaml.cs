﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using HyPlayer.Classes;
using HyPlayer.Controls;
using HyPlayer.HyPlayControl;
using NeteaseCloudMusicApi;

#endregion

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace HyPlayer.Pages;

/// <summary>
///     可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class Home : Page
{
    private static List<string> RandomSlogen = new()
    {
        "用音乐开启新的一天吧",
        "戴上耳机 享受新的一天吧"
    };

    public Home()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (Common.Logined)
            LoadLoginedContent();
        else LoadUnLoginedContent();
        HyPlayList.OnLoginDone += LoadLoginedContent;
    }

    private void LoadUnLoginedContent()
    {
        LoadRanklist();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        HyPlayList.OnLoginDone -= LoadLoginedContent;
        UnLoginedContent.Children.Clear();
        //DailySongContainer.Children.Clear();
        RankPlayList.Children.Clear();
        //MySongHis.Children.Clear();
        RecommendSongListContainer.Children.Clear();
    }

    private async void LoadLoginedContent()
    {
        UnLoginedContent.Visibility = Visibility.Collapsed;
        LoginedContent.Visibility = Visibility.Visible;
        TbHelloUserName.Text = Common.LoginedUser.name;
        //我们直接Batch吧
        try
        {
            var ret = await Common.ncapi.RequestAsync(
                CloudMusicApiProviders.Batch,
                new Dictionary<string, object>
                {
                    { "/api/toplist", "{}" }
                    //{ "/weapi/v1/discovery/recommend/resource","{}" }   //这个走不了Batch
                }
            );

            //每日推荐加载部分 - 日推不加载
            /*
            var rcmdSongsJson = ret["/api/v3/discovery/recommend/songs"]["data"]["dailySongs"].ToArray();
            Common.ListedSongs.Clear();
            DailySongContainer.Children.Clear();
            var NowSongPanel = new StackPanel();
            for (var c = 0; c < rcmdSongsJson.Length; c++)
            {
                if (c % 3 == 0)
                {
                    NowSongPanel = new StackPanel
                    { Orientation = Orientation.Vertical, Height = DailySongContainer.Height, Width = 600 };
                    DailySongContainer.Children.Add(NowSongPanel);
                }

                var nownc = NCSong.CreateFromJson(rcmdSongsJson[c]);
                Common.ListedSongs.Add(nownc);
                NowSongPanel.Children.Add(new SingleNCSong(nownc, c, true, true,
                    rcmdSongsJson[c]["reason"].ToString()));
            }
            */

            //榜单
            RankPlayList.Children.Clear();
            foreach (var bditem in ret["/api/toplist"]["list"])
                RankPlayList.Children.Add(new PlaylistItem(NCPlayList.CreateFromJson(bditem)));


            //推荐歌单加载部分 - 优先级稍微靠后下
            try
            {
                var ret1 = await Common.ncapi.RequestAsync(CloudMusicApiProviders.RecommendResource);

                RecommendSongListContainer.Children.Clear();
                foreach (var item in ret1["recommend"])
                    RecommendSongListContainer.Children.Add(new PlaylistItem(NCPlayList.CreateFromJson(item)));
            }
            catch (Exception ex)
            {
                Common.AddToTeachingTipLists(ex.Message, (ex.InnerException ?? new Exception()).Message);
            }
        }
        catch (Exception ex)
        {
            Common.AddToTeachingTipLists(ex.Message, (ex.InnerException ?? new Exception()).Message);
        }
    }

    public async void LoadRanklist()
    {
        try
        {
            var json = await Common.ncapi.RequestAsync(CloudMusicApiProviders.Toplist);

            foreach (var PlaylistItemJson in json["list"].ToArray())
            {
                var ncp = NCPlayList.CreateFromJson(PlaylistItemJson);
                RankList.Children.Add(new PlaylistItem(ncp));
            }
        }
        catch (Exception ex)
        {
            Common.AddToTeachingTipLists(ex.Message, (ex.InnerException ?? new Exception()).Message);
        }
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        PersonalFM.InitPersonalFM();
    }

    private void dailyRcmTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        Common.NavigatePage(typeof(SongListDetail), new NCPlayList
        {
            cover = "ms-appx:/Assets/icon.png",
            creater = new NCUser
            {
                avatar = "https://p1.music.126.net/KxePid7qTvt6V2iYVy-rYQ==/109951165050882728.jpg",
                id = "1",
                name = "网易云音乐",
                signature = "网易云音乐官方账号 "
            },
            plid = "-666",
            subscribed = false,
            name = "每日歌曲推荐",
            desc = "根据你的口味生成，每天6:00更新"
        });
    }

    private void FMTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        PersonalFM.InitPersonalFM();
    }

    private void LikedSongListTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e        )
    {
        Common.NavigatePage(typeof(SongListDetail), Common.MySongLists[0].plid);

    }
}