using System.ComponentModel;
using System.Globalization;
using UpInBlue.Leonardo.WinUI.Controls;

namespace UpInBlue.Leonardo.WinUI.Tests;

public sealed class SummaryLineTests
{
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

    [Fact]
    public void ComposesTheSentenceWithMiddleDots()
    {
        var sentence = SummaryLineText.Compose("Since your last visit:", Items((SummaryKind.New, 2), (SummaryKind.CheckedOut, 1), (SummaryKind.Conflict, 1)), English);

        Assert.Equal("Since your last visit: 2 new · 1 checked out · 1 conflict", sentence.Text);
    }

    [Fact]
    public void ScreenReadersHearCommasInsteadOfDots()
    {
        var sentence = SummaryLineText.Compose("Since your last visit:", Items((SummaryKind.New, 2), (SummaryKind.CheckedOut, 1), (SummaryKind.Conflict, 1)), English);

        Assert.Equal("Since your last visit: 2 new, 1 checked out, 1 conflict", sentence.SpokenText);
    }

    [Fact]
    public void PhrasesWithZeroAreLeftOut()
    {
        var sentence = SummaryLineText.Compose("Since your last visit:", Items((SummaryKind.New, 0), (SummaryKind.CheckedOut, 1), (SummaryKind.Conflict, 0)), English);

        Assert.Equal("Since your last visit: 1 checked out", sentence.Text);
        Assert.Single(sentence.Phrases);
    }

    [Fact]
    public void HidesWhenAllCountsAreZero()
    {
        var items = Items((SummaryKind.New, 0), (SummaryKind.Conflict, 0));

        Assert.False(SummaryLineText.IsVisible(items));
        Assert.False(SummaryLineText.Compose("Since your last visit:", items, English).IsVisible);
        Assert.Equal(string.Empty, SummaryLineText.Compose("Since your last visit:", items, English).Text);
    }

    [Fact]
    public void HidesWithoutItems()
    {
        Assert.False(SummaryLineText.IsVisible([]));
        Assert.False(SummaryLineText.IsVisible(null));
        Assert.False(SummaryLineText.Compose("Prefix", null, English).IsVisible);
    }

    [Fact]
    public void ShowsWhenOneCountIsAboveZero()
    {
        Assert.True(SummaryLineText.IsVisible(Items((SummaryKind.New, 0), (SummaryKind.Conflict, 3))));
    }

    [Fact]
    public void ConflictsArePlural()
    {
        var sentence = SummaryLineText.Compose(null, Items((SummaryKind.New, 3), (SummaryKind.CheckedOut, 2), (SummaryKind.Conflict, 2)), English);

        Assert.Equal("3 new · 2 checked out · 2 conflicts", sentence.Text);
    }

    [Fact]
    public void CustomPhrasesUseTheirLabels()
    {
        var waiting = new SummaryLineItem { Kind = SummaryKind.Custom, Count = 1, Label = "request waiting", PluralLabel = "requests waiting" };

        Assert.Equal("1 request waiting", waiting.Phrase(English));
        waiting.Count = 4;
        Assert.Equal("4 requests waiting", waiting.Phrase(English));
    }

    [Fact]
    public void CustomLabelWithoutPluralStaysTheSame()
    {
        var item = new SummaryLineItem { Kind = SummaryKind.Custom, Count = 2, Label = "approved" };

        Assert.Equal("2 approved", item.Phrase(English));
    }

    [Fact]
    public void LargeCountsUseTheCultureOfTheNumbers()
    {
        var item = new SummaryLineItem { Kind = SummaryKind.New, Count = 1250 };

        Assert.Equal("1,250 new", item.Phrase(English));
        Assert.Equal("1.250 new", item.Phrase(CultureInfo.GetCultureInfo("de-DE")));
    }

    [Fact]
    public void NegativeCountsBecomeZero()
    {
        var item = new SummaryLineItem { Count = -3 };

        Assert.Equal(0, item.Count);
        Assert.False(SummaryLineText.IsVisible([item]));
    }

    [Fact]
    public void PrefixIsOptional()
    {
        var sentence = SummaryLineText.Compose("  ", Items((SummaryKind.New, 2)), English);

        Assert.Equal("2 new", sentence.Text);
    }

    [Fact]
    public void SeveralPhrasesCanBeActiveAtOnce()
    {
        var items = Items((SummaryKind.New, 2), (SummaryKind.CheckedOut, 1), (SummaryKind.Conflict, 1));

        items[0].IsActive = true;
        items[2].IsActive = true;
        var sentence = SummaryLineText.Compose("Since your last visit:", items, English);

        Assert.True(items[0].IsActive);
        Assert.False(items[1].IsActive);
        Assert.True(items[2].IsActive);
        Assert.Equal(3, sentence.Phrases.Count);
    }

    [Fact]
    public void ChangingAnItemRaisesPropertyChanged()
    {
        var item = new SummaryLineItem();
        var changed = new List<string?>();
        item.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        item.Count = 2;
        item.IsActive = true;
        item.IsActive = true;

        Assert.Equal([nameof(SummaryLineItem.Count), nameof(SummaryLineItem.IsActive)], changed);
    }

    private static SummaryLineItem[] Items(params (SummaryKind Kind, int Count)[] items)
        => items.Select(item => new SummaryLineItem { Kind = item.Kind, Count = item.Count }).ToArray();
}
