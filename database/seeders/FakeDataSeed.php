<?php

namespace Database\Seeders;

use Illuminate\Database\Seeder;

use App\Models\Category;
use App\Models\User;
use App\Models\Location;
use App\Models\File;
use App\Models\UserDocument;
use App\Models\UserPermission;
use App\Models\UserRequest;
use App\Models\Message;
use App\Models\Request;
use App\Models\RequestTransaction;
use App\Models\Transaction;
use App\Models\Event;
use App\Models\EventTransaction;
use App\Models\UserEvent;

class FakeDataSeed extends Seeder
{
    /**
     * Run the database seeds.
     *
     * @return void
     */
    public function run()
    {
        $categories = Category::factory(15)->create();
        $users=User::factory(25)->create();
        $locations=Location::factory(30)->create();
        $files=File::factory(25)->create();
        $request=Request::factory(10)->create();
        $requestTransactions=RequestTransaction::factory(100)->create();
        $events=Event::factory(10)->create();
        $eventTransactions=EventTransaction::factory(100)->create();
        $transaction=Transaction::factory(100)->create();
        $userDocument=UserDocument::factory(25)->create();
        $userPermission=UserPermission::factory(25)->create();
        $userRequest=UserRequest::factory(15)->create();
        $userEvent=UserEvent::factory(10)->create();
        $userMessages=Message::factory(150)->create();
        

    


    }
}
