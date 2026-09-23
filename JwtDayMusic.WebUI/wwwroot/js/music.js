(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        initSidebarToggle();
        initCategoryFilters();
        initSearch();
        initFavoriteButtons();
        initFollowButtons();
        initArtistGenreFilter();
        initAddToPlaylist();
        initPlaylistPageActions();
        initPlayer();
    });

    function initSidebarToggle() {
        var toggleBtn = document.querySelector('[data-sidebar-toggle]');
        var sidebar = document.querySelector('.sidebar');
        var backdrop = document.querySelector('.sidebar-backdrop');

        if (!toggleBtn || !sidebar || !backdrop) return;

        function close() {
            sidebar.classList.remove('is-open');
            backdrop.classList.remove('is-open');
        }

        toggleBtn.addEventListener('click', function () {
            sidebar.classList.toggle('is-open');
            backdrop.classList.toggle('is-open');
        });

        backdrop.addEventListener('click', close);
    }

    function initCategoryFilters() {
        var pills = document.querySelectorAll('[data-category-pill]');
        var cards = document.querySelectorAll('[data-song-card]');
        var emptyState = document.querySelector('[data-empty-state]');
        var searchInput = document.querySelector('[data-song-search]');

        if (!pills.length) return;

        pills.forEach(function (pill) {
            pill.addEventListener('click', function () {
                pills.forEach(function (p) { p.classList.remove('active'); });
                pill.classList.add('active');
                applyFilters(cards, emptyState, pill.dataset.categoryPill, searchInput ? searchInput.value : '');
            });
        });
    }

    function initSearch() {
        var searchInput = document.querySelector('[data-song-search]');
        var cards = document.querySelectorAll('[data-song-card]');
        var emptyState = document.querySelector('[data-empty-state]');
        var activePill = document.querySelector('[data-category-pill].active');

        if (!searchInput) return;

        searchInput.addEventListener('input', function () {
            var category = activePill ? activePill.dataset.categoryPill : 'all';
            applyFilters(cards, emptyState, category, searchInput.value);
        });
    }

    function applyFilters(cards, emptyState, category, query) {
        var normalizedQuery = (query || '').trim().toLowerCase();
        var visibleCount = 0;

        cards.forEach(function (card) {
            var matchesCategory =
                category === 'all' ||
                card.dataset.category === category ||
                card.dataset.badge === category;

            var haystack = (card.dataset.title + ' ' + card.dataset.artist).toLowerCase();
            var matchesQuery = !normalizedQuery || haystack.indexOf(normalizedQuery) !== -1;

            var isVisible = matchesCategory && matchesQuery;
            card.style.display = isVisible ? '' : 'none';
            if (isVisible) visibleCount++;
        });

        if (emptyState) {
            emptyState.style.display = visibleCount === 0 ? 'block' : 'none';
        }
    }

    function initFollowButtons() {
        document.querySelectorAll('[data-follow-btn]').forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                e.stopPropagation();
                e.preventDefault();

                var card = btn.closest('[data-artist-card]');
                var artistId = card ? card.dataset.artistId : null;
                if (!artistId) return;

                btn.disabled = true;

                fetch('/Artist/ToggleFollow?artistId=' + encodeURIComponent(artistId), {
                    method: 'POST',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                })
                    .then(function (res) { return res.json(); })
                    .then(function (result) {
                        btn.disabled = false;

                        if (!result.success) {
                            showToast(result.message || '\u0130\u015flem ger\u00e7ekle\u015ftirilemedi.', 'error');
                            return;
                        }

                        var label = btn.querySelector('.follow-btn-label');
                        var followingPage = card ? card.closest('[data-following-page]') : null;

                        if (!result.following && followingPage) {
                            // Takip Ettiklerim sayfasýndayýz, takibi býrakýnca kartý listeden kaldýr
                            card.style.transition = 'opacity 200ms ease, transform 200ms ease';
                            card.style.opacity = '0';
                            card.style.transform = 'scale(0.96)';
                            setTimeout(function () { card.remove(); }, 200);
                            return;
                        }

                        if (result.following) {
                            btn.classList.add('is-following');
                            if (label) label.textContent = 'Takip Ediliyor';
                        } else {
                            btn.classList.remove('is-following');
                            if (label) label.textContent = 'Takip Et';
                        }
                    })
                    .catch(function () {
                        btn.disabled = false;
                        showToast('Sunucuya ula\u015f\u0131lamad\u0131.', 'error');
                    });
            });
        });
    }

    function initArtistGenreFilter() {
        var pills = document.querySelectorAll('[data-artist-genre-pill]');
        var cards = document.querySelectorAll('[data-artist-card]');
        var emptyState = document.querySelector('[data-artist-empty-state]');

        if (!pills.length) return;

        pills.forEach(function (pill) {
            pill.addEventListener('click', function () {
                pills.forEach(function (p) { p.classList.remove('active'); });
                pill.classList.add('active');

                var selected = pill.dataset.artistGenrePill;
                var visibleCount = 0;

                cards.forEach(function (card) {
                    var genres = card.dataset.genres || '';
                    var isVisible = selected === 'all' || genres.indexOf(selected) !== -1;
                    card.style.display = isVisible ? '' : 'none';
                    if (isVisible) visibleCount++;
                });

                if (emptyState) {
                    emptyState.style.display = visibleCount === 0 ? 'block' : 'none';
                }
            });
        });
    }

    function initAddToPlaylist() {
        var openDropdown = null;

        function closeDropdown() {
            if (openDropdown) {
                openDropdown.remove();
                openDropdown = null;
            }
        }

        document.addEventListener('click', function (e) {
            if (openDropdown && !e.target.closest('.playlist-dropdown') && !e.target.closest('[data-add-to-playlist-btn]')) {
                closeDropdown();
            }
        });

        document.querySelectorAll('[data-add-to-playlist-btn]').forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                e.stopPropagation();
                e.preventDefault();

                var card = btn.closest('[data-song-card]');
                var songId = card ? card.dataset.songId : null;
                if (!songId) return;

                if (openDropdown) {
                    closeDropdown();
                    return;
                }

                var rect = btn.getBoundingClientRect();
                var dropdown = document.createElement('div');
                dropdown.className = 'playlist-dropdown';
                dropdown.style.top = (rect.bottom + 6) + 'px';
                dropdown.style.left = Math.max(8, rect.right - 220) + 'px';
                dropdown.innerHTML = '<div class="playlist-dropdown-empty">Y\u00fckleniyor...</div>';
                document.body.appendChild(dropdown);
                openDropdown = dropdown;

                fetch('/Playlist/MyPlaylistsJson')
                    .then(function (res) { return res.json(); })
                    .then(function (playlists) {
                        dropdown.innerHTML = '';

                        if (!playlists.length) {
                            var empty = document.createElement('div');
                            empty.className = 'playlist-dropdown-empty';
                            empty.textContent = 'Hen\u00fcz \u00e7alma listeniz yok.';
                            dropdown.appendChild(empty);
                        } else {
                            playlists.forEach(function (p) {
                                var item = document.createElement('button');
                                item.type = 'button';
                                item.className = 'playlist-dropdown-item';
                                item.innerHTML = '<i class="bi bi-music-note-list"></i> ' + escapeHtml(p.name);
                                item.addEventListener('click', function () {
                                    addSongToPlaylist(p.id, songId, p.name);
                                    closeDropdown();
                                });
                                dropdown.appendChild(item);
                            });
                        }

                        var divider = document.createElement('div');
                        divider.className = 'playlist-dropdown-divider';
                        dropdown.appendChild(divider);

                        var newItem = document.createElement('button');
                        newItem.type = 'button';
                        newItem.className = 'playlist-dropdown-item playlist-dropdown-new';
                        newItem.innerHTML = '<i class="bi bi-plus-circle"></i> Yeni \u00e7alma listesi';
                        newItem.addEventListener('click', function () {
                            closeDropdown();
                            createPlaylistAndAddSong(songId);
                        });
                        dropdown.appendChild(newItem);
                    })
                    .catch(function () {
                        dropdown.innerHTML = '<div class="playlist-dropdown-empty">Y\u00fcklenemedi.</div>';
                    });
            });
        });
    }

    function addSongToPlaylist(playlistId, songId, playlistName) {
        var body = new URLSearchParams({ playlistId: playlistId, songId: songId });
        fetch('/Playlist/AddSong', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'X-Requested-With': 'XMLHttpRequest' },
            body: body.toString()
        })
            .then(function (res) { return res.json(); })
            .then(function (result) {
                if (result.success) {
                    showToast('"' + playlistName + '" listesine eklendi.', 'info');
                } else {
                    showToast(result.message || 'Eklenemedi.', 'error');
                }
            })
            .catch(function () {
                showToast('Sunucuya ula\u015f\u0131lamad\u0131.', 'error');
            });
    }

    function createPlaylistAndAddSong(songId) {
        var name = prompt('Yeni \u00e7alma listesi ad\u0131:');
        if (!name || !name.trim()) return;

        fetch('/Playlist/Create', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'X-Requested-With': 'XMLHttpRequest' },
            body: 'name=' + encodeURIComponent(name.trim())
        })
            .then(function (res) { return res.json(); })
            .then(function (result) {
                if (!result.success) {
                    showToast(result.message || 'Olu\u015fturulamad\u0131.', 'error');
                    return;
                }

                if (songId) {
                    addSongToPlaylist(result.id, songId, result.name);
                } else {
                    location.reload();
                }
            })
            .catch(function () {
                showToast('Sunucuya ula\u015f\u0131lamad\u0131.', 'error');
            });
    }

    function initPlaylistPageActions() {
        var createBtn = document.querySelector('[data-create-playlist]');
        if (createBtn) {
            createBtn.addEventListener('click', function () {
                createPlaylistAndAddSong(null);
            });
        }

        document.querySelectorAll('[data-remove-from-playlist]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var container = document.querySelector('[data-playlist-page]');
                var playlistId = container ? container.dataset.playlistPage : null;
                var songId = btn.dataset.removeFromPlaylist;
                if (!playlistId || !songId) return;

                var body = new URLSearchParams({ playlistId: playlistId, songId: songId });
                fetch('/Playlist/RemoveSong', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'X-Requested-With': 'XMLHttpRequest' },
                    body: body.toString()
                })
                    .then(function (res) { return res.json(); })
                    .then(function (result) {
                        if (!result.success) {
                            showToast(result.message || '\u00c7\u0131kar\u0131lamad\u0131.', 'error');
                            return;
                        }
                        var wrap = btn.closest('.playlist-song-wrap');
                        if (wrap) {
                            wrap.style.transition = 'opacity 200ms ease';
                            wrap.style.opacity = '0';
                            setTimeout(function () { wrap.remove(); }, 200);
                        }
                    })
                    .catch(function () {
                        showToast('Sunucuya ula\u015f\u0131lamad\u0131.', 'error');
                    });
            });
        });

        document.querySelectorAll('[data-delete-playlist]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var playlistId = btn.dataset.deletePlaylist;
                if (!confirm('Bu \u00e7alma listesini silmek istedi\u011finize emin misiniz?')) return;

                fetch('/Playlist/Delete', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'X-Requested-With': 'XMLHttpRequest' },
                    body: 'id=' + encodeURIComponent(playlistId)
                })
                    .then(function (res) { return res.json(); })
                    .then(function (result) {
                        if (result.success) {
                            window.location.href = '/Playlist/Index';
                        } else {
                            showToast(result.message || 'Silinemedi.', 'error');
                        }
                    })
                    .catch(function () {
                        showToast('Sunucuya ula\u015f\u0131lamad\u0131.', 'error');
                    });
            });
        });
    }

    function escapeHtml(text) {
        var div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    function initFavoriteButtons() {
        document.querySelectorAll('[data-fav-btn]').forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                e.stopPropagation();

                var card = btn.closest('[data-song-card]');
                var songId = card ? card.dataset.songId : null;
                if (!songId) return;

                btn.disabled = true;

                fetch('/Song/ToggleFavorite?songId=' + encodeURIComponent(songId), {
                    method: 'POST',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                })
                    .then(function (res) { return res.json(); })
                    .then(function (result) {
                        btn.disabled = false;

                        if (!result.success) {
                            showToast(result.message || '\u0130\u015flem ger\u00e7ekle\u015ftirilemedi.', 'error');
                            return;
                        }

                        var icon = btn.querySelector('i');
                        var favoritesPage = card ? card.closest('[data-favorites-page]') : null;

                        if (!result.favorited && favoritesPage) {
                            // Beðendiklerim sayfasýndayýz, favoriden çýkýnca kartý kaldýr
                            card.style.transition = 'opacity 200ms ease, transform 200ms ease';
                            card.style.opacity = '0';
                            card.style.transform = 'scale(0.96)';
                            setTimeout(function () { card.remove(); }, 200);
                            return;
                        }

                        if (result.favorited) {
                            btn.classList.add('is-favorited');
                            if (icon) icon.className = 'bi bi-heart-fill';
                        } else {
                            btn.classList.remove('is-favorited');
                            if (icon) icon.className = 'bi bi-heart';
                        }
                    })
                    .catch(function () {
                        btn.disabled = false;
                        showToast('Sunucuya ula\u015f\u0131lamad\u0131.', 'error');
                    });
            });
        });
    }

    function initPlayer() {
        var audio = document.getElementById('audioPlayer');
        var playBtn = document.querySelector('[data-player-play]');
        var playIcon = playBtn ? playBtn.querySelector('i') : null;
        var progress = document.querySelector('[data-player-progress]');
        var currentTimeLabel = document.querySelector('[data-player-current-time]');
        var durationLabel = document.querySelector('[data-player-duration]');
        var trackCover = document.querySelector('[data-player-cover]');
        var trackTitle = document.querySelector('[data-player-title]');
        var trackArtist = document.querySelector('[data-player-artist]');
        var shuffleBtn = document.querySelector('[data-player-shuffle]');
        var repeatBtn = document.querySelector('[data-player-repeat]');
        var volumeSlider = document.querySelector('[data-player-volume]');

        if (!audio) return;

        var isSeeking = false;

        function formatTime(totalSeconds) {
            if (!isFinite(totalSeconds) || totalSeconds < 0) totalSeconds = 0;
            var minutes = Math.floor(totalSeconds / 60);
            var seconds = Math.floor(totalSeconds % 60).toString().padStart(2, '0');
            return minutes + ':' + seconds;
        }

        function setPlayingIcon(playing) {
            if (!playIcon) return;
            playIcon.className = playing ? 'bi bi-pause-fill' : 'bi bi-play-fill';
        }

        function loadAndPlay(song) {
            if (trackTitle) trackTitle.textContent = song.title;
            if (trackArtist) trackArtist.textContent = song.artist;
            if (trackCover) trackCover.src = song.cover;

            audio.src = song.audioUrl;
            audio.currentTime = 0;
            if (progress) progress.value = 0;
            if (currentTimeLabel) currentTimeLabel.textContent = '0:00';

            var playPromise = audio.play();
            if (playPromise && playPromise.catch) {
                playPromise.catch(function () {
                    showToast('Ses dosyas\u0131 oynat\u0131lamad\u0131. Ba\u011flant\u0131y\u0131 kontrol edin.', 'error');
                    setPlayingIcon(false);
                });
            }
        }

        // ---- Audio element eventleri: gerçek çalma süresine/ilerlemeye göre UI güncelleniyor ----

        audio.addEventListener('loadedmetadata', function () {
            if (durationLabel) durationLabel.textContent = formatTime(audio.duration);
        });

        audio.addEventListener('timeupdate', function () {
            if (isSeeking) return;
            if (progress && audio.duration) {
                progress.value = (audio.currentTime / audio.duration) * 100;
            }
            if (currentTimeLabel) currentTimeLabel.textContent = formatTime(audio.currentTime);
        });

        audio.addEventListener('play', function () { setPlayingIcon(true); });
        audio.addEventListener('pause', function () { setPlayingIcon(false); });
        audio.addEventListener('ended', function () { setPlayingIcon(false); });

        audio.addEventListener('error', function () {
            showToast('Ses dosyas\u0131 y\u00fcklenemedi.', 'error');
            setPlayingIcon(false);
        });

        // ---- Kontroller ----

        if (playBtn) {
            playBtn.addEventListener('click', function () {
                if (!audio.src) return; // henüz bir þarký seçilmedi
                if (audio.paused) {
                    audio.play();
                } else {
                    audio.pause();
                }
            });
        }

        if (progress) {
            progress.addEventListener('input', function () {
                isSeeking = true;
                if (audio.duration) {
                    var previewTime = (progress.value / 100) * audio.duration;
                    if (currentTimeLabel) currentTimeLabel.textContent = formatTime(previewTime);
                }
            });

            progress.addEventListener('change', function () {
                if (audio.duration) {
                    audio.currentTime = (progress.value / 100) * audio.duration;
                }
                isSeeking = false;
            });
        }

        if (shuffleBtn) {
            shuffleBtn.addEventListener('click', function () {
                shuffleBtn.classList.toggle('is-active');
            });
        }

        if (repeatBtn) {
            repeatBtn.addEventListener('click', function () {
                var isActive = repeatBtn.classList.toggle('is-active');
                audio.loop = isActive;
            });
        }

        if (volumeSlider) {
            audio.volume = Number(volumeSlider.value) / 100;

            volumeSlider.addEventListener('input', function () {
                var icon = document.querySelector('[data-player-volume-icon]');
                var value = Number(volumeSlider.value);
                audio.volume = value / 100;
                if (!icon) return;
                icon.className = 'bi ' + (value === 0 ? 'bi-volume-mute' : value < 50 ? 'bi-volume-down' : 'bi-volume-up');
            });
        }

        // ---- Kart üzerindeki play butonlarý: API'ye sorup, izin varsa gerçek sesi baþlat ----

        document.querySelectorAll('[data-play-song]').forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                e.stopPropagation();
                var card = btn.closest('[data-song-card]');
                if (!card) return;

                var songId = card.dataset.songId;
                btn.disabled = true;

                fetch('/Song/Play?songId=' + encodeURIComponent(songId), {
                    method: 'GET',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                })
                    .then(function (res) { return res.json(); })
                    .then(function (result) {
                        btn.disabled = false;

                        if (!result.success) {
                            showToast(result.message || 'Bu \u015fark\u0131 oynat\u0131lamad\u0131.', 'error');
                            return;
                        }

                        loadAndPlay({
                            title: card.dataset.title,
                            artist: card.dataset.artist,
                            cover: card.dataset.cover,
                            audioUrl: result.audioUrl
                        });
                    })
                    .catch(function () {
                        btn.disabled = false;
                        showToast('Sunucuya ula\u015f\u0131lamad\u0131.', 'error');
                    });
            });
        });
    }

    function showToast(message, type) {
        var existing = document.querySelector('.app-toast');
        if (existing) existing.remove();

        var toast = document.createElement('div');
        toast.className = 'app-toast app-toast-' + (type || 'info');
        toast.textContent = message;
        document.body.appendChild(toast);

        requestAnimationFrame(function () {
            toast.classList.add('is-visible');
        });

        setTimeout(function () {
            toast.classList.remove('is-visible');
            setTimeout(function () { toast.remove(); }, 250);
        }, 3200);
    }
})();